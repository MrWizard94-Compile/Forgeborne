"""Capture a silent reload clip with actual Unity renders and a terminal receipt."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--instance', required=True)
parser.add_argument('--output-dir', required=True)
args = parser.parse_args()
destination = (ROOT / args.output_dir).resolve()
if ROOT / 'docs/evidence' not in destination.parents:
    raise ValueError('Destination must be a new folder inside docs/evidence')
if destination.exists() and any(destination.iterdir()):
    raise ValueError('Retain previous evidence; use an empty directory')


def call(path):
    result = subprocess.run([sys.executable, str(ROOT / 'tools/unity-mcp-client.py'),
                             '--instance', args.instance, '--tool', 'execute_code', '--code-file', str(path)],
                            cwd=ROOT, capture_output=True, text=True)
    data = json.loads(result.stdout)
    if result.returncode or not data.get('success'):
        raise RuntimeError(data)
    return data['data']['result']


with tempfile.TemporaryDirectory(prefix='forgeborne-reload-clip-') as temporary:
    temporary = Path(temporary)
    script = temporary / 'capture.cs'
    script.write_text((ROOT / 'tools/unity-reload-clip.cs').read_text(encoding='utf-8-sig')
                      .replace('__OUTPUT_DIRECTORY__', destination.relative_to(ROOT).as_posix()), encoding='utf-8')
    status = temporary / 'status.cs'
    status.write_text('return new {active=SessionState.GetBool("Forgeborne.ReloadClip.Active",false),'
                      'frames=SessionState.GetInt("Forgeborne.ReloadClip.Frames",0),'
                      'directory=SessionState.GetString("Forgeborne.ReloadClip.Directory",""),'
                      'heartbeatAge=EditorApplication.timeSinceStartup-SessionState.GetFloat("Forgeborne.ReloadClip.Heartbeat",0)};', encoding='utf-8')
    print(json.dumps(call(script)), flush=True)
    deadline = time.monotonic() + 600
    while time.monotonic() < deadline:
        current = call(status)
        print(json.dumps(current), flush=True)
        if not current['active']:
            report = json.loads((destination / 'clip-report.json').read_text())
            if not report['success']:
                raise RuntimeError(report['failure'])
            assert report['actualFrames'] == report['expectedFrames']
            assert all(frame['normalFramesCrossed'] > 1 for frame in report['frames'])
            print(json.dumps({key: value for key, value in report.items() if key != 'frames'}), flush=True)
            break
        time.sleep(5)
    else:
        raise TimeoutError('Existing Unity capture is still owned; inspect/cancel its handle before retrying')
