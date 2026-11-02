"""Capture the original rig only after actual Unity player/render frame boundaries."""
from pathlib import Path
import argparse,json,subprocess,sys,tempfile

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument("--instance",required=True,help="Explicit connected Unity MCP instance")
parser.add_argument("--inspection",action="store_true",help="Diagnostic side views rather than normal reload samples")
args=parser.parse_args()
INSTANCE=args.instance
CLIENT=ROOT/"tools/unity-mcp-client.py"
DEST=ROOT/"docs/evidence/2026-11-02/carbine-0127"
template=(ROOT/"tools"/("unity-carbine-inspection-set.cs" if args.inspection else "unity-carbine-set-pose.cs")).read_text(encoding="utf-8-sig")
def call(path):
    process=subprocess.run([sys.executable,str(CLIENT),"--instance",INSTANCE,"--tool","execute_code","--code-file",str(path)],
                           cwd=ROOT,capture_output=True,text=True)
    if not process.stdout.strip():raise RuntimeError(process.stderr)
    value=json.loads(process.stdout)
    if not value.get("success"):raise RuntimeError(json.dumps(value))
    return value["data"]["result"]

records=[]
with tempfile.TemporaryDirectory(prefix="forgeborne-carbine-poses-") as private:
    script=Path(private)/"set-pose.cs"
    try:
        for value in ((65,-65) if args.inspection else (0,.12,.30,.42,.65,.8)):
            script.write_text(template.replace("__YAW__" if args.inspection else "__SECONDS__",f"{value:.2f}f"),encoding="utf-8")
            setter=call(script)
            captured=call(ROOT/"tools/unity-carbine-rendered-capture.cs")
            records.append({"setter":setter,"capture":captured})
            print(json.dumps(captured),flush=True)
    finally:
        call(ROOT/"tools/unity-carbine-reset-pose.cs")
output="unity-model-inspection.json" if args.inspection else "unity-rendered-poses.json"
(DEST/output).write_text(json.dumps({"actualFrameBoundaryCaptures":records,"diagnosticSideViews":args.inspection},indent=2)+"\n",encoding="utf-8")
