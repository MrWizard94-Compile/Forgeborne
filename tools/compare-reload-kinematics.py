"""Compare actual rig receipts at the same physical sampling interval."""
import argparse
import json
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('baseline', type=Path)
parser.add_argument('current', type=Path)
parser.add_argument('--output', type=Path)
args = parser.parse_args()


def load(path):
    receipt = json.loads(path.read_text(encoding='utf-8-sig'))
    if not receipt.get('success'):
        raise ValueError('Actual Unity probe failed: ' + str(path))
    result = receipt['data']['result']
    if not result['canonicalStateUnchanged'] or not result['sameSimulation']:
        raise ValueError('Probe did not preserve game state and ownership')
    return result


baseline, current = load(args.baseline), load(args.current)
if baseline['sampleHz'] != 240 or current['sampleHz'] != 240:
    raise ValueError('Both receipts must use the fixed 240 Hz probe')
if abs(baseline['actualStepSeconds'] - current['actualStepSeconds']) > 1e-8:
    raise ValueError('Sampling intervals differ')
metrics = []
for key, budget in [('cameraSpacePalmPeakSpeedMetresPerSecond', 3.5),
                    ('cameraSpacePalmPeakAccelerationMetresPerSecondSquared', 60.0)]:
    before, after = baseline[key], current[key]
    metrics.append(dict(metric=key, baseline=before, current=after, workingArtBudget=budget,
                        reductionPercent=(1 - after / before) * 100,
                        passed=0 < after <= budget and after < before))
report = dict(passed=all(row['passed'] for row in metrics), baselineVersion=baseline['version'],
              currentVersion=current['version'], baselineDurationSeconds=baseline['durationSeconds'],
              currentDurationSeconds=current['durationSeconds'], sampleHz=240, metrics=metrics,
              limits='Working animation budgets for actual camera-space rig references. '
                     'Not universal biomechanics, visible-mesh clearance, native performance or art/fun acceptance.')
text = json.dumps(report, indent=2) + '\n'
if args.output:
    with args.output.open('x', encoding='utf-8') as output:
        output.write(text)
print(text, end='')
raise SystemExit(0 if report['passed'] else 1)
