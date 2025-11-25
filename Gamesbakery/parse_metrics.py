import json
import sys

with open('report.json') as f:
    data = json.load(f)
failed = False
for file, metrics in data.items():
    cc = metrics.get('cyclomatic_complexity', 0)
    if cc > 10:
        print(f"File {file} doesn't satisfy cyclomatic complexity: {cc}")
        failed = True
    else:
        print(f"File {file} satisfies cyclomatic complexity: {cc}")
if failed:
    sys.exit(1)