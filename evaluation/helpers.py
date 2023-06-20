import dataclasses
import json

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors


COLORS = list(mcolors.TABLEAU_COLORS.values())

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

    assert eps_df.query("terminationCause == 1")[
        eps_df.query("terminationCause == 1").accName.isnull()
    ].empty  # otherwise there exist acc violations that are not properly tracked

    return eps_df


def get_comp_eps_df(eps_df):
    comp_eps_df = eps_df.query("localEpisodeNumber == 0")[
        [
            "compositeEpisodeNumber",
            "globalSuccess",
            "globalSteps",
        ]
    ]

    return comp_eps_df


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


def plot_per_action(actions, means, ylabel: str, title: str):
    x = np.arange(len(actions))  # the label locations
    width = 0.33  # the width of the bars
    multiplier = 0

    fig, ax = plt.subplots(layout="constrained")

    for attribute, measurement in means.items():
        offset = width * multiplier
        rects = ax.bar(x + offset, measurement, width * 0.9, label=attribute)
        ax.bar_label(rects, padding=3)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(means) - 1), actions)
    ax.legend(loc="upper left", ncols=3)
    # ax.set_ylim(0, 1)

    plt.show()


def boxplot_per_action(actions, datas, ylabel, title):
    # TODO: add labels
    x = np.arange(len(actions))  # the label locations
    width = 0.33  # the width of the bars
    multiplier = 0
    bps_list = []

    fig, ax = plt.subplots(layout="constrained")

    for i, (attribute, data) in enumerate(datas.items()):
        offset = width * multiplier
        # print(data)
        bps = ax.boxplot(
            data,
            positions=x + offset,
            widths=width * 0.9,
            labels=[attribute] * len(data),
            notch=True,
            patch_artist=True,
            showfliers=True,
            boxprops=dict(facecolor=COLORS[i]),
        )
        bps_list.append(bps)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(datas) - 1), actions)
    ax.legend(
        [bps["boxes"][0] for bps in bps_list], datas.keys(), loc="upper left", ncols=3
    )
    # ax.set_ylim(0, 1)

    plt.show()


def gather_statistics(comp_eps_df):
    stats = Statistics(
        comp_eps_df.globalSuccess.mean(),
        comp_eps_df.globalSteps.min(),
        comp_eps_df.globalSteps.max(),
        comp_eps_df.globalSteps.mean(),
        comp_eps_df.globalSteps.std(),
    )

    return stats


def get_acc_violation_rate(eps_df: pd.DataFrame, actions):
    local_success_rate = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        local_success_rate.append((action_df.terminationCause == 1).mean())
    return local_success_rate


def get_avg_num_eps_per_action(eps_df: pd.DataFrame, actions):
    # eps_per_action = eps_df.groupby(["action", "compositeEpisodeNumber"]).count().terminationCause.reset_index("compositeEpisodeNumber").groupby("action").terminationCause.mean()
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = (
            action_df.groupby("compositeEpisodeNumber").terminationCause.count().mean()
        )
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


def get_num_eps_per_action(eps_df: pd.DataFrame, actions):
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").terminationCause.count()
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


def get_avg_total_steps_per_action(eps_df: pd.DataFrame, actions):
    steps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").localSteps.sum().mean()
        steps_per_action_list.append(avg_eps)
    return steps_per_action_list
