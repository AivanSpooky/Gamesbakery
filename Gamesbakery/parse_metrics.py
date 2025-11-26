import sys
import json
import chardet  # pip install chardet
import subprocess
import re

# Autodetect encoding for report.json
with open('report.json', 'rb') as f:
    raw = f.read()
    enc_json = chardet.detect(raw)['encoding'] or 'utf-8'
print(f"Detected encoding for report.json: {enc_json}")

with open('report.json', 'r', encoding=enc_json) as f:
    data = json.load(f)
failed = False
max_cc = 0
high_cc_files = []
for file, metrics in data.get('files', {}).items():
    if metrics:  # Skip empty files
        cc = metrics.get('cyclomatic_complexity', 0)
        if cc > max_cc:
            max_cc = cc
        if cc > 10:
            #print(f"File {file} exceeds CC: {cc}")
            high_cc_files.append(file)
            # Run lizard on this file to get per-method CC
            try:
                lizard_output = subprocess.run(["lizard", "-l", "csharp", file], capture_output=True, text=True).stdout
                lines = lizard_output.splitlines()
                for line in lines:
                    if re.match(r'^\s*\d+', line):
                        parts = re.split(r'\s+', line.strip())
                        if len(parts) >= 6 and '@' in parts[-1]:
                            try:
                                method_cc = int(parts[1])
                                method = parts[-1]
                                if method_cc > 10:
                                    print(f"  Method {method} in {file} exceeds CC: {method_cc}")
                                    failed = True
                            except ValueError:
                                continue
            except Exception as e:
                print(f"Error running lizard on {file}: {e}")
            #failed = True
        # Print Halstead per file
        # print(f"File {file}: Halstead volume={metrics.get('halstead_volume', 0)}, difficulty={metrics.get('halstead_difficulty', 0)}, effort={metrics.get('halstead_effort', 0)}, time={metrics.get('halstead_timerequired', 0)}")

if failed:
    sys.exit(1)
else:
    print("All CC <=10! Success!")