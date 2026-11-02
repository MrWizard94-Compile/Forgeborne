"""Sample the real paused support-hand sequence across normal render frames."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--instance', required=True)
parser.add_argument('--output-dir', required=True, help='New repository-relative evidence/render folder')
parser.add_argument('--phases', type=float, nargs='+', default=[0, .16, .28, .44, .62, .76, .84, .90, 1])
args = parser.parse_args()
if any(not 0 <= phase <= 1 for phase in args.phases):
    raise ValueError('Reload phases must be between zero and one')
destination = (ROOT / args.output_dir).resolve()
if ROOT / 'docs/evidence' not in destination.parents:
    raise ValueError('Render destination must be inside docs/evidence')
destination.mkdir(parents=True, exist_ok=True)
if any(destination.iterdir()):
    raise ValueError('Use an empty render destination to retain earlier evidence')


def call(path):
    process = subprocess.run([sys.executable, str(ROOT / 'tools/unity-mcp-client.py'),
                              '--instance', args.instance, '--tool', 'execute_code',
                              '--code-file', str(path)], cwd=ROOT, capture_output=True, text=True)
    if not process.stdout.strip():
        raise RuntimeError(process.stderr)
    result = json.loads(process.stdout)
    if process.returncode or not result.get('success'):
        raise RuntimeError(json.dumps(result))
    return result['data']['result']


setter = (ROOT / 'tools/unity-weapon-framing-set.cs').read_text(encoding='utf-8-sig')
capture = (ROOT / 'tools/unity-weapon-framing-capture.cs').read_text(encoding='utf-8-sig')
old_destination = '../../docs/evidence/2026-11-02/framing-0128/candidates'
assert capture.count(old_destination) == 1
capture = capture.replace(old_destination, '../../' + destination.relative_to(ROOT).as_posix())
records = []
with tempfile.TemporaryDirectory(prefix='forgeborne-hand-motion-') as temporary:
    temporary = Path(temporary)
    status = temporary / 'status.cs'
    status.write_text('return new {duration=WeaponViewModel.ReloadPresentationSeconds};', encoding='utf-8')
    duration = call(status)['duration']
    capture_path = temporary / 'capture.cs'
    capture_path.write_text(capture, encoding='utf-8')
    setter_path = temporary / 'set.cs'
    try:
        for phase in args.phases:
            label = 'reload-phase-' + f'{phase:.2f}'.replace('.', '-')
            text = setter.replace('__LABEL__', label).replace('__AGE__', f'{duration * phase:.6f}f')
            for token, value in dict(SHOT='false', X='.19f', Y='-.10f', Z='.55f', YAW='-12f').items():
                text = text.replace('__' + token + '__', value)
            setter_path.write_text(text, encoding='utf-8')
            result = dict(phase=phase, seconds=duration * phase, setter=call(setter_path), capture=call(capture_path))
            records.append(result)
            print(json.dumps(result['capture']), flush=True)
    finally:
        call(ROOT / 'tools/unity-weapon-framing-reset.cs')
(destination / 'rendered-motion.json').write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')
