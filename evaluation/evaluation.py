import dataclasses
import json
import os
import pandas as pd
import matplotlib.pyplot as plt

runId = "testRunId"

filePath = f"evaluation/stats/{runId}/statistics.json"

action_termination_cause = [
    "PostConditionReached",
    "ACCViolated",
    "LocalReset",
    "GlobalReset",
]


@dataclasses.dataclass
class Statistics:
    success_rate: float
    min_global_steps: int
    max_global_steps: int
    mean_global_steps: float
    std_global_steps: float


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
        action_statistics = btepisode["actionStatistics"]
        for action in action_statistics:
            action_statistic = action_statistics[action]
            episodes = action_statistic["episodes"]
            for j in range(len(episodes)):
                episode = episodes[j]
                episode["action"] = action
                episode["compositeEpisodeNumber"] = i

                if (
                    episode["terminationCause"] == 1
                ):  # ACC violated. Should be equivalent to episode["accInfo"] is not None
                    for key, value in episode["accInfo"].items():
                        episode[key] = value
                del episode["accInfo"]

                episode["globalSuccess"] = global_success[i]
                episode["globalSteps"] = gloabl_steps[i]

                augmented_episodes.append(episode)

    eps_df = pd.DataFrame(augmented_episodes)
    eps_df.sort_values(
        by=["compositeEpisodeNumber", "localEpisodeNumber"], inplace=True
    )
    return eps_df


eps_df = load_repr1_to_eps(filePath)

# print(eps_df)
# print(eps_df.columns)

actions = eps_df.action.unique()
# print(actions)
accs = eps_df.query("terminationCause == 1").groupby("action").accName.unique()
# print(accs)
print("compositeEpisodeNumber:", eps_df.compositeEpisodeNumber.max() + 1)
assert eps_df.query("terminationCause == 1")[
    eps_df.query("terminationCause == 1").accName.isnull()
].empty  # otherwise there exist acc violations that are not properly tracked

comp_eps_df = eps_df.query("localEpisodeNumber == 0")[
    [
        "compositeEpisodeNumber",
        "globalSuccess",
        "globalSteps",
    ]
]
print(comp_eps_df)
stats = Statistics(
    comp_eps_df.globalSuccess.mean(),
    comp_eps_df.globalSteps.min(),
    comp_eps_df.globalSteps.max(),
    comp_eps_df.globalSteps.mean(),
    comp_eps_df.globalSteps.std(),
)
print(stats)
# plt.hist(comp_eps_df.globalSteps)
# plt.show()

for action in actions:
    print(action)
    action_df = eps_df.query("action == @action")
    print(action_df.terminationCause.value_counts())
    if not action_df.query("terminationCause == 1").empty:
        print(action_df.query("terminationCause == 1").accName.value_counts())
    print("success rate:", action_df.globalSuccess.mean())
    print("min local steps:", action_df.localSteps.min())
    print("max local steps:", action_df.localSteps.max())
    print("mean local steps:", action_df.localSteps.mean())
    print("std local steps:", action_df.localSteps.std())
    print()
