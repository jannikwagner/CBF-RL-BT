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

# could instead also be done in unity
augmented_episodes = []

for i in range(len(data)):
    btepisode = data[i]
    actionStatistics = btepisode["actionStatistics"]
    localEpisodeNumber = 0
    for action in actionStatistics:
        actionStatistic = actionStatistics[action]
        episodes = actionStatistic["episodes"]
        for j in range(len(episodes)):
            episode = episodes[j]
            episode["action"] = action
            episode["compositeEpisodeNumber"] = i
            episode["localEpisodeNumber"] = localEpisodeNumber
            localEpisodeNumber += 1
            augmented_episodes.append(episode)

df = pd.DataFrame(augmented_episodes)

print(df)
