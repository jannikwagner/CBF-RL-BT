import json
import os
import pandas as pd

runId = "testRunId"

filePath = f"evaluation/stats/{runId}/statistics.json"

# print(os.listdir())

with open(filePath, "r") as f:
    data = json.load(f)
    # print(data)

df = pd.DataFrame(data)

steps = df.steps
global_success = df.globalSuccess
