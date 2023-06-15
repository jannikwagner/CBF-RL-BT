import json
import os
import pandas as pd

runId = "testRunId"

filePath = f"evaluation/stats/{runId}/statistics.json"

action_termination_cause = [
    "PostConditionReached",
    "ACCViolated",
    "LocalReset",
    "GlobalReset",
]

# print(os.listdir())

with open(filePath, "r") as f:
    data = json.load(f)
    # print(data)

comp_ep_df = pd.DataFrame(data)

gloabl_steps = comp_ep_df.steps
global_success = comp_ep_df.globalSuccess

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

            if episode["accInfo"]:
                for key, value in episode["accInfo"].items():
                    episode["acc."+key] = value
            del episode["accInfo"]

            episode["globalSuccess"] = global_success[i]
            episode["globalSteps"] = gloabl_steps[i]

            # episode["localEpisodeNumber"] = localEpisodeNumber
            # localEpisodeNumber += 1  # this is wrong because the episodes are sorted by action

            augmented_episodes.append(episode)

eps_df = pd.DataFrame(augmented_episodes)

print(eps_df)
