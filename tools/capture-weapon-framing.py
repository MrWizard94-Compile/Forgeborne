"""Compare actual paused FPS framing, restoring offsets and flags after each sample."""
import argparse,json,subprocess,sys,tempfile
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--instance',required=True)
parser.add_argument('--selected-only',action='store_true',help='Capture only the selected presentation')
mode=parser.add_mutually_exclusive_group()
mode.add_argument('--reload',action='store_true',help='Sample the actual 0.42-second presentation pose')
mode.add_argument('--shot',action='store_true',help='Sample peak recoil and trigger pose with paused presentation time')
args=parser.parse_args()
template=(ROOT/'tools/unity-weapon-framing-set.cs').read_text(encoding='utf-8-sig')
destination=ROOT/'docs/evidence/2026-11-02/framing-0128'
destination.mkdir(parents=True,exist_ok=True)

def call(path):
    p=subprocess.run([sys.executable,str(ROOT/'tools/unity-mcp-client.py'),'--instance',args.instance,
                      '--tool','execute_code','--code-file',str(path)],capture_output=True,text=True,cwd=ROOT)
    if not p.stdout.strip():raise RuntimeError(p.stderr)
    value=json.loads(p.stdout)
    if p.returncode or not value.get('success'):raise RuntimeError(json.dumps(value))
    return value['data']['result']

records=[]
with tempfile.TemporaryDirectory(prefix='forgeborne-framing-') as temporary:
    script=Path(temporary)/'set.cs'
    try:
        candidates=[('legacy-placement',(.20,-.17,.70),0),('shoulder-angled',(.20,-.10,.50),-15),('balanced-angled',(.20,-.10,.55),-10),('selected',(.19,-.10,.55),-12)]
        for label,offset,yaw in (candidates[-1:] if args.selected_only else candidates):
            label+= '-reload' if args.reload else '-shot' if args.shot else '-rest'
            text=template.replace('__LABEL__',label).replace('__AGE__','.42f' if args.reload else '0f')
            text=text.replace('__SHOT__','true' if args.shot else 'false')
            text=text.replace('__YAW__',f'{yaw:.2f}f')
            for axis,value in zip('XYZ',offset):text=text.replace('__'+axis+'__',f'{value:.3f}f')
            script.write_text(text,encoding='utf-8')
            setter=call(script);capture=call(ROOT/'tools/unity-weapon-framing-capture.cs')
            records.append({'setter':setter,'capture':capture});print(json.dumps(capture),flush=True)
    finally:call(ROOT/'tools/unity-weapon-framing-reset.cs')
kind='reload' if args.reload else 'shot' if args.shot else 'rest'
(destination/(kind+('-selected' if args.selected_only else '-comparison')+'.json')).write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
