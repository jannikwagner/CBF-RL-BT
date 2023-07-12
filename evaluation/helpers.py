import dataclasses
import json
from typing import Sequence, Tuple

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

    comp_eps_df.sort_values(by="compositeEpisodeNumber", inplace=True)

    comp_eps_df.index = comp_eps_df.compositeEpisodeNumber

    local_step_sum = eps_df.groupby("compositeEpisodeNumber").localSteps.sum()

    local_episodes_count = eps_df.groupby(
        "compositeEpisodeNumber"
    ).terminationCause.count()

    comp_eps_df["localStepsSum"] = local_step_sum
    comp_eps_df["localEpisodesCount"] = local_episodes_count

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


def plot_per_action(
    actions: Sequence[str],
    labels: Sequence[str],
    values: Sequence,
    ylabel: str,
    title: str,
):
    x = np.arange(len(actions))  # the label locations
    width = 1 / (len(values) + 1)  # the width of the bars
    multiplier = 0

    fig, ax = plt.subplots(layout="constrained")

    for label, data in zip(labels, values):
        offset = width * multiplier
        rects = ax.bar(x + offset, data, width * 0.9, label=label)
        ax.bar_label(rects, padding=3)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(values) - 1), actions, rotation=45, fontsize=8)
    ax.legend(loc="upper left", ncols=3)
    # ax.set_ylim(0, 1)

    plt.show()


def boxplot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    boxplot_per_action(action_accs, labels, data, ylabel, title)


def boxplot_per_action(
    actions: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
):
    x = np.arange(len(actions))  # the label locations
    width = 1 / (len(datas) + 1)  # the width of the bars
    multiplier = 0
    bps_list = []

    fig, ax = plt.subplots(layout="constrained")

    for i, (label, data) in enumerate(zip(labels, datas)):
        offset = width * multiplier
        bps = ax.boxplot(
            data,
            positions=x + offset,
            widths=width * 0.9,
            patch_artist=True,
            boxprops=dict(facecolor=COLORS[i]),
            medianprops=dict(color="black"),
            showfliers=True,
            showmeans=True,
            meanline=True,
            notch=True,
            bootstrap=1000,
        )
        bps_list.append(bps)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(datas) - 1), actions, rotation=45, fontsize=8)
    ax.legend([bps["boxes"][0] for bps in bps_list], labels, loc="upper left", ncols=3)

    plt.show()


def global_boxplot(labels: Sequence[str], data: Sequence, ylabel: str, title: str):
    x = np.arange(len(labels))  # the label locations
    width = 0.5  # the width of the bars

    fig, ax = plt.subplots(layout="constrained")

    bps = ax.boxplot(
        data,
        positions=x,
        widths=width,
        patch_artist=True,
        boxprops=dict(facecolor=COLORS[0]),
        medianprops=dict(color="black"),
        showfliers=True,
        showmeans=True,
        meanline=True,
        notch=True,
        bootstrap=1000,
    )

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x, labels, rotation=45, fontsize=8)

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


def get_acc_violation_rate_per_action(eps_df: pd.DataFrame, actions):
    local_success_rate = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        local_success_rate.append((action_df.terminationCause == 1).mean())
    return local_success_rate


ACC_STEPS_TO_RECOVER_THRESHOLD = 0


def get_acc_steps_to_recover_per_acc(
    eps_df: pd.DataFrame, action_acc_tuples, threshold=ACC_STEPS_TO_RECOVER_THRESHOLD
):
    steps_list = []
    for action, acc in action_acc_tuples:
        acc_df = eps_df.query(
            "action == @action & terminationCause == 1 & accName == @acc"
        )
        acc_steps_to_recover = get_acc_steps_to_recover(acc_df, threshold)
        steps_list.append(acc_steps_to_recover)

    return steps_list


def get_acc_steps_to_recover_per_action(
    eps_df: pd.DataFrame, actions, threshold=ACC_STEPS_TO_RECOVER_THRESHOLD
):
    steps_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        acc_steps_to_recover = get_acc_steps_to_recover(action_df, threshold)
        steps_list.append(acc_steps_to_recover)
    return steps_list


def get_acc_steps_to_recover(eps_df, threshold=ACC_STEPS_TO_RECOVER_THRESHOLD):
    # threshold is a hack to ignore erroneous acc violations
    # TODO: fix bug that causes erroneous acc violations
    acc_steps_to_recover = eps_df.query(
        "terminationCause == 1 & accRecovered & accStepsToRecover >= @threshold"
    ).accStepsToRecover

    return acc_steps_to_recover


def get_num_eps_per_action(eps_df: pd.DataFrame, actions):
    # eps_per_action = eps_df.groupby(["action", "compositeEpisodeNumber"]).count().terminationCause.reset_index("compositeEpisodeNumber").groupby("action").terminationCause
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").terminationCause.count()
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


def get_avg_num_eps_per_action(eps_df: pd.DataFrame, actions):
    return [eps.mean() for eps in get_num_eps_per_action(eps_df, actions)]


def get_total_steps_per_action(eps_df: pd.DataFrame, actions):
    # summed up local steps (episode lengths) within a composite episode per action
    steps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").localSteps.sum()
        steps_per_action_list.append(avg_eps)
    return steps_per_action_list


def get_avg_total_steps_per_action(eps_df: pd.DataFrame, actions):
    return [steps.mean() for steps in get_total_steps_per_action(eps_df, actions)]


def get_local_steps_per_action(eps_df: pd.DataFrame, actions):
    steps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.localSteps
        steps_per_action_list.append(avg_eps)
    return steps_per_action_list
