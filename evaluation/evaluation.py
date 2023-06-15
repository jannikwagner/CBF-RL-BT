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

def load_repr1_to_eps(filePath):
    with open(filePath, "r") as f:
        data = json.load(f)
    # print(data)

    comp_ep_df = pd.DataFrame(data)

    gloabl_steps = comp_ep_df.globalSteps
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

                if episode["terminationCause"] == 1:  # ACC violated. Should be equivalent to episode["accInfo"] is not None
                    for key, value in episode["accInfo"].items():
                        episode[key] = value
                del episode["accInfo"]

                episode["globalSuccess"] = global_success[i]
                episode["globalSteps"] = gloabl_steps[i]

                augmented_episodes.append(episode)

    eps_df = pd.DataFrame(augmented_episodes)
    eps_df.sort_values(by=["compositeEpisodeNumber", "localEpisodeNumber"], inplace=True)
    return eps_df

eps_df = load_repr1_to_eps(filePath)

print(eps_df)
print(eps_df.columns)

actions = eps_df.action.unique()
print(actions)
accs = eps_df.query("terminationCause == 1").groupby("action").accName.unique()
print(accs)
print(eps_df.compositeEpisodeNumber.max())
assert eps_df.query("terminationCause == 1")[eps_df.query("terminationCause == 1").accName.isnull()].empty  # otherwise there exist acc violations that are not properly tracked

comp_eps_df = eps_df.query("localEpisodeNumber == 0")[['compositeEpisodeNumber', 'globalSuccess', 'globalSteps',]]
print(comp_eps_df)
print(comp_eps_df.globalSteps.mean())
print(comp_eps_df.globalSteps.min())
print(comp_eps_df.globalSteps.max())

print(eps_df.query("action == 'MoveOverBridge'").terminationCause.value_counts())
