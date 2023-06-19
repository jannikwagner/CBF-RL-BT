import dataclasses
import json
import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

run_id = "testRunId"

file_path = f"evaluation/stats/{run_id}/statisticsWOCBF.json"

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


def load_repr1_to_eps(file_path):
    with open(file_path, "r") as f:
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


eps_df = load_repr1_to_eps(file_path)

actions = eps_df.action.unique()
accs = eps_df.query("terminationCause == 1").groupby("action").accName.unique()
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


def print_action_summary(eps_df, action):
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


# for action in actions:
#     print_action_summary(eps_df, action)


file_path2 = f"evaluation/stats/{run_id}/statisticsWCBF.json"
eps_df2 = load_repr1_to_eps(file_path2)


def get_acc_violation_rate(eps_df, actions):
    local_success_rate = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        local_success_rate.append((action_df.terminationCause == 1).mean())
    return local_success_rate


acc_violation_rates = {
    "WOCBF": get_acc_violation_rate(eps_df, actions),
    "WCBF": get_acc_violation_rate(eps_df2, actions),
}


def plot_per_action(actions, acc_violation_rates, ylabel, title):
    x = np.arange(len(actions))  # the label locations
    width = 0.25  # the width of the bars
    multiplier = 0

    fig, ax = plt.subplots(layout="constrained")

    for attribute, measurement in acc_violation_rates.items():
        offset = width * multiplier
        rects = ax.bar(x + offset, measurement, width, label=attribute)
        ax.bar_label(rects, padding=3)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width, actions)
    ax.legend(loc="upper left", ncols=3)
    # ax.set_ylim(0, 1)

    plt.show()


ylabel = "acc violation rate"
title = "ACC violation rates"

plot_per_action(actions, acc_violation_rates, ylabel, title)

# eps_per_action = eps_df.groupby(["action", "compositeEpisodeNumber"]).count().terminationCause.reset_index("compositeEpisodeNumber").groupby("action").terminationCause.mean()


def get_avg_eps_per_action(eps_df, actions):
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = (
            action_df.groupby("compositeEpisodeNumber").count().terminationCause.mean()
        )
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


eps_per_action = {
    "WOCBF": get_avg_eps_per_action(eps_df, actions),
    "WCBF": get_avg_eps_per_action(eps_df2, actions),
}


plot_per_action(actions, eps_per_action, "Episodes", "Episodes per action")


def get_steps_per_action(eps_df, actions):
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").localSteps.sum().mean()
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


steps_per_action = {
    "WOCBF": get_steps_per_action(eps_df, actions),
    "WCBF": get_steps_per_action(eps_df2, actions),
}

plot_per_action(actions, steps_per_action, "Steps", "Steps per action")
