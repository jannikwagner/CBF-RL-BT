import dataclasses
import json
from typing import Mapping, Sequence, Tuple
from enum import Enum
import os

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors
import seaborn as sns

PLOT_FOLDER = "evaluation/plots/testRunId"

COLORS = list(mcolors.TABLEAU_COLORS.values())

action_termination_causes = [
    "PostConditionReached",
    "ACCViolated",
    "LocalReset",
    "GlobalReset",
    "HigherPostConditionReached",
]
action_termination_causes_dict = dict(
    zip(action_termination_causes, range(len(action_termination_causes)))
)
ActionTerminationCause = Enum("ActionTerminationCause", action_termination_causes_dict)


@dataclasses.dataclass
class Statistics:
    success_rate: float
    min_global_steps: int
    max_global_steps: int
    mean_global_steps: float
    std_global_steps: float
    min_local_episodes: int
    max_local_episodes: int
    mean_local_episodes: float
    std_local_episodes: float
    termination_cause_rates: Mapping[str, float]

    def to_latex(self):
        return self.to_df().to_latex()

    def to_df(self):
        df = pd.DataFrame(dataclasses.asdict(self))
        return df


def load_repr1_to_eps(file_path):
    with open(file_path, "r") as f:
        data = json.load(f)

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

    assert (
        eps_df.query("terminationCause == 1").empty
        or eps_df.query("terminationCause == 1")[
            eps_df.query("terminationCause == 1").accName.isnull()
        ].empty
    )  # otherwise there exist acc violations that are not properly tracked

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

    acc_violation_count = (
        eps_df.query("terminationCause == 1")
        .groupby("compositeEpisodeNumber")
        .terminationCause.count()
    )

    pc_reached_count = (
        eps_df.query("terminationCause == 0")
        .groupby("compositeEpisodeNumber")
        .terminationCause.count()
    )

    comp_eps_df["localStepsSum"] = local_step_sum
    comp_eps_df["localEpisodesCount"] = local_episodes_count
    comp_eps_df["accViolationCount"] = 0
    comp_eps_df.loc[
        acc_violation_count.index, "accViolationCount"
    ] = acc_violation_count
    comp_eps_df["pcReachedCount"] = 0
    comp_eps_df.loc[pc_reached_count.index, "pcReachedCount"] = pc_reached_count

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


def bars_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    values: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    bars_per_group(
        action_accs, labels, values, ylabel, title, show=show, figsize=figsize
    )


def bars_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    values: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    x = np.arange(len(groups))  # the label locations
    width = 1 / (len(values) + 1)  # the width of the bars
    multiplier = 0

    fig, ax = plt.subplots(layout="constrained")
    fig.set_figwidth(figsize[0])
    fig.set_figheight(figsize[1])

    for label, data in zip(labels, values):
        offset = width * multiplier
        rects = ax.bar(x + offset, data, width * 0.9, label=label)
        # ax.bar_label(rects, padding=1)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(values) - 1), groups, rotation=45, fontsize=8)
    ax.legend(loc="upper right", ncols=3)
    # ax.set_ylim(0, 1)

    display(f"gplot.{title}", show)


def display(title, show):
    if show:
        plt.show()
    else:
        os.makedirs(PLOT_FOLDER, exist_ok=True)
        path = os.path.join(PLOT_FOLDER, f"{title}.pdf")
        plt.savefig(path, bbox_inches="tight")
    plt.cla()
    plt.close()


def boxplot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    boxplot_per_group(
        action_accs, labels, data, ylabel, title, show=show, figsize=figsize
    )


def violinplot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    violinplot_per_group(
        action_accs, labels, data, ylabel, title, show=show, figsize=figsize
    )


def plot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    boxplot_per_group(
        action_accs, labels, data, ylabel, title, show=show, figsize=figsize
    )
    violinplot_per_group(
        action_accs, labels, data, ylabel, title, show=show, figsize=figsize
    )


BOXPLOT_SETTINGS = dict(
    patch_artist=True,
    medianprops=dict(color="black"),
    showfliers=False,
    showmeans=True,
    meanline=True,
    meanprops=dict(color="red"),
    # whis=[5, 95]
    # notch=True,
    # bootstrap=1000,
)


def plot_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    boxplot_per_group(groups, labels, datas, ylabel, title, show, figsize)
    violinplot_per_group(groups, labels, datas, ylabel, title, show, figsize)


def boxplot_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    x = np.arange(len(groups))  # the label locations
    width = 1 / (len(datas) + 1)  # the width of the bars
    multiplier = 0
    bps_list = []

    fig, ax = plt.subplots(layout="constrained")
    fig.set_figwidth(figsize[0])
    fig.set_figheight(figsize[1])

    for i, data in enumerate(datas):
        offset = width * multiplier
        bps = ax.boxplot(
            data,
            positions=x + offset,
            widths=width * 0.9,
            boxprops=dict(facecolor=COLORS[i]),
            **BOXPLOT_SETTINGS,
        )
        bps_list.append(bps)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    # plt.yscale('log')
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x + width / 2 * (len(datas) - 1), groups, rotation=45, fontsize=8)
    ax.legend([bps["boxes"][0] for bps in bps_list], labels, loc="upper left", ncols=3)

    display(f"gbp.{title}", show)


def violinplot_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(6, 4),
):
    xs = np.arange(len(groups))  # the label locations
    width = 1 / (len(datas))  # the width of the bars
    multiplier = 0
    bps_list = []

    fig, ax = plt.subplots(layout="constrained")
    fig.set_figwidth(figsize[0])
    fig.set_figheight(figsize[1])

    for i, data in enumerate(datas):
        nonempty_xs = np.array([x for x, d in zip(xs, data) if len(d) > 0])
        nonempty_data = [d for d in data if len(d) > 0]

        offset = width * multiplier

        bps = ax.violinplot(
            nonempty_data,
            positions=nonempty_xs + offset,
            widths=width * 0.9,
            showmeans=False,
            showmedians=True,
            quantiles=[[0.1, 0.9]] * len(nonempty_xs),
        )

        for key in bps.keys():
            if key == "bodies":
                for pc in bps[key]:
                    pc.set_facecolor(COLORS[i])
                    pc.set_edgecolor(COLORS[i])
                    pc.set_alpha(0.5)
            else:
                bps[key].set_color(COLORS[i])

        bps_list.append(bps)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    # plt.yscale('log')
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(xs + width / 2 * (len(datas) - 1), groups, rotation=45, fontsize=8)
    ax.legend([bps["bodies"][0] for bps in bps_list], labels, loc="upper left", ncols=3)

    display(f"gviolin.{title}", show)


def global_plot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(3, 3),
):
    global_boxplot(labels, data, ylabel, title, show, figsize)
    global_violinplot(labels, data, ylabel, title, show, figsize)


def global_boxplot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(3, 3),
):
    x = np.arange(len(labels))  # the label locations
    width = 0.5  # the width of the bars

    fig, ax = plt.subplots(layout="constrained")
    fig.set_figwidth(figsize[0])
    fig.set_figheight(figsize[1])

    bps = ax.boxplot(
        data,
        positions=x,
        widths=width,
        boxprops=dict(facecolor=COLORS[0]),
        **BOXPLOT_SETTINGS,
    )

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x, labels, rotation=45, fontsize=8)

    display(f"bp.{title}", show)


def global_violinplot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
    figsize=(3, 3),
):
    x = np.arange(len(labels))  # the label locations
    width = 1  # the width of the bars

    fig, ax = plt.subplots(layout="constrained")
    fig.set_figwidth(figsize[0])
    fig.set_figheight(figsize[1])

    bps = ax.violinplot(
        data,
        positions=x,
        widths=width,
        showmeans=False,
        showmedians=True,
        quantiles=[[0.1, 0.9]] * len(data),
    )

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x, labels, rotation=45, fontsize=8)

    display(f"violin.{title}", show)


def global_hist(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    show=False,
):
    sns.histplot(
        dict(zip(labels, data)), color=COLORS, kde=True, label=ylabel, stat="density"
    )

    # Add some text for labels, title and custom x-axis tick labels, etc.
    # sns(ylabel)
    # sns.
    plt.title(title)
    # plt.set_xticks(x, labels, rotation=45, fontsize=8)

    display(f"hist.{title}", show)


def gather_statistics(comp_eps_df, eps_df):
    termination_cause_rates = get_termination_cause_rates(eps_df)
    stats = dict(
        success_rate=comp_eps_df.globalSuccess.mean(),
        min_global_steps=comp_eps_df.globalSteps.min(),
        max_global_steps=comp_eps_df.globalSteps.max(),
        mean_global_steps=comp_eps_df.globalSteps.mean(),
        std_global_steps=comp_eps_df.globalSteps.std(),
        min_local_episodes=comp_eps_df.localEpisodesCount.min(),
        max_local_episodes=comp_eps_df.localEpisodesCount.max(),
        mean_local_episodes=comp_eps_df.localEpisodesCount.mean(),
        std_local_episodes=comp_eps_df.localEpisodesCount.std(),
        # **dict(zip(action_termination_causes, termination_cause_rates)),
    )
    df = pd.DataFrame([stats])
    return df


def get_acc_violation_rate(eps_df: pd.DataFrame):
    return (eps_df.terminationCause == 1).mean()


def get_acc_violation_rate_per_action(eps_df: pd.DataFrame, actions: Sequence[str]):
    local_success_rate = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        local_success_rate.append(get_acc_violation_rate(action_df))
    return local_success_rate


def get_acc_violation_rate_per_acc(
    eps_df: pd.DataFrame, action_acc_tuples: Sequence[Tuple[str, str],]
):
    local_success_rate = []
    for action, acc in action_acc_tuples:
        num_eps_with_this_action = len(eps_df.query("action == @action"))
        num_eps_with_this_action_and_this_acc_violation = len(
            eps_df.query("action == @action & terminationCause == 1 & accName == @acc")
        )
        local_success_rate.append(
            num_eps_with_this_action_and_this_acc_violation / num_eps_with_this_action
        )
    return local_success_rate


ACC_STEPS_TO_RECOVER_THRESHOLD = 0


def get_acc_steps_to_recover_per_acc(
    eps_df: pd.DataFrame,
    action_acc_tuples: Sequence[Tuple[str, str]],
    threshold=ACC_STEPS_TO_RECOVER_THRESHOLD,
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


def get_termination_cause_rates(df):
    counts = df.groupby("terminationCause").action.count()
    total = counts.sum()
    rates = counts / total
    for cause in action_termination_causes_dict.values():
        if cause not in rates:
            rates[cause] = 0
    return [rates[cause] for cause in action_termination_causes_dict.values()]
