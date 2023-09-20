import dataclasses
import json
from typing import Mapping, Sequence, Tuple
from enum import Enum
import os

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors
import scipy.stats as st
import seaborn as sns

PLOT_FOLDER = "evaluation/plots/"

COLORS = list(mcolors.TABLEAU_COLORS.values())

VIOLINPLOT_AXIS_PERCENTILES = (0, 95)
VIOLINPLOT_AXLIM_MARGIN = 0.04
VIOLINPLOT_QUANTILE_LINES = [0.25, 0.75]

skill_termination_causes = [
    "PostConditionReached",
    "ACCViolated",
    "LocalReset",
    "GlobalReset",
    "HigherPostConditionReached",
]
action_termination_causes_dict = dict(
    zip(skill_termination_causes, range(len(skill_termination_causes)))
)
SkillTerminationCause = Enum("ActionTerminationCause", action_termination_causes_dict)


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

    eps_df.index = eps_df.index.sort_values()

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


def print_skill_summary(eps_df, action):
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
    store_folder=None,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    bars_per_group(
        action_accs,
        labels,
        values,
        ylabel,
        title,
        store_folder=store_folder,
        figsize=figsize,
    )


def bars_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    values: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
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

    display(f"gplot.{title}", store_folder)


def display(title, store_folder=None):
    if not store_folder:
        plt.show()
    else:
        file_path = os.path.join(PLOT_FOLDER, store_folder, f"{title}.pdf")
        os.makedirs(os.path.dirname(file_path), exist_ok=True)
        plt.savefig(file_path, bbox_inches="tight")
    plt.cla()
    plt.close()


def boxplot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    boxplot_per_group(
        action_accs,
        labels,
        data,
        ylabel,
        title,
        store_folder=store_folder,
        figsize=figsize,
    )


def violinplot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    violinplot_per_group(
        action_accs,
        labels,
        data,
        ylabel,
        title,
        store_folder=store_folder,
        figsize=figsize,
    )


def plot_per_acc(
    action_acc_tuples: Sequence[Tuple[str, str]],
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
    figsize=(6, 4),
):
    action_accs = [f"{action}.{acc}" for (action, acc) in action_acc_tuples]
    boxplot_per_group(
        action_accs,
        labels,
        data,
        ylabel,
        title,
        store_folder=store_folder,
        figsize=figsize,
    )
    violinplot_per_group(
        action_accs,
        labels,
        data,
        ylabel,
        title,
        store_folder=store_folder,
        figsize=figsize,
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
    store_folder=None,
    figsize=(6, 4),
):
    boxplot_per_group(groups, labels, datas, ylabel, title, store_folder, figsize)
    violinplot_per_group(groups, labels, datas, ylabel, title, store_folder, figsize)


def boxplot_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
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

    display(f"gbp.{title}", store_folder)


def violinplot_per_group(
    groups: Sequence[str],
    labels: Sequence[str],
    datas: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
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
        # nonempty_data = [
        #     within_percentiles(d, VIOLINPLOT_PERCENTILES) for d in nonempty_data
        # ]

        offset = width * multiplier

        bps = ax.violinplot(
            nonempty_data,
            positions=nonempty_xs + offset,
            widths=width * 0.9,
            showmeans=True,
            showmedians=True,
            quantiles=[VIOLINPLOT_QUANTILE_LINES] * len(nonempty_xs),
        )

        color_violinplot(i, bps)

        bps_list.append(bps)
        multiplier += 1

    # Add some text for labels, title and custom x-axis tick labels, etc.
    # plt.yscale('log')

    ymin = min(
        np.percentile(d, VIOLINPLOT_AXIS_PERCENTILES[0])
        for data in datas
        for d in data
        if len(d) > 0
    )
    ymax = max(
        np.percentile(d, VIOLINPLOT_AXIS_PERCENTILES[1])
        for data in datas
        for d in data
        if len(d) > 0
    )
    diff = ymax - ymin
    margin = VIOLINPLOT_AXLIM_MARGIN * diff
    ax.set_ylim(ymin - margin, ymax + margin)
    ax.set_xlim(-len(groups) / 10, len(groups) - 1 + 0.5 + len(groups) / 10)

    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(xs + width / 2 * (len(datas) - 1), groups, rotation=45, fontsize=8)
    ax.legend([bps["bodies"][0] for bps in bps_list], labels, loc="upper left", ncols=3)

    display(f"gviolin.{title}", store_folder)


def color_violinplot(i, bps):
    for key in bps.keys():
        if key == "bodies":
            for pc in bps[key]:
                pc.set_facecolor(COLORS[i])
                pc.set_edgecolor(COLORS[i])
                pc.set_alpha(0.5)
        elif key == "cmeans":
            bps[key].set_color("black")
        else:
            bps[key].set_color(COLORS[i])


def global_plot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
    figsize=(3, 3),
):
    global_boxplot(labels, data, ylabel, title, store_folder, figsize)
    global_violinplot(labels, data, ylabel, title, store_folder, figsize)


def global_boxplot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
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

    display(f"bp.{title}", store_folder)


def global_violinplot(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
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
        showmeans=True,
        showmedians=True,
        quantiles=[VIOLINPLOT_QUANTILE_LINES] * len(data),
    )
    color_violinplot(0, bps)

    # Add some text for labels, title and custom x-axis tick labels, etc.
    ymin = min(
        np.percentile(d, VIOLINPLOT_AXIS_PERCENTILES[0]) for d in data if len(d) > 0
    )
    ymax = max(
        np.percentile(d, VIOLINPLOT_AXIS_PERCENTILES[1]) for d in data if len(d) > 0
    )
    diff = ymax - ymin
    margin = VIOLINPLOT_AXLIM_MARGIN * diff
    ax.set_ylim(ymin - margin, ymax + margin)

    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.set_xticks(x, labels, rotation=45, fontsize=8)

    display(f"violin.{title}", store_folder)


def global_hist(
    labels: Sequence[str],
    data: Sequence,
    ylabel: str,
    title: str,
    store_folder=None,
):
    sns.histplot(
        dict(zip(labels, data)), color=COLORS, kde=True, label=ylabel, stat="density"
    )

    # Add some text for labels, title and custom x-axis tick labels, etc.
    # sns(ylabel)
    # sns.
    plt.title(title)
    # plt.set_xticks(x, labels, rotation=45, fontsize=8)

    display(f"hist.{title}", store_folder)


def gather_statistics(comp_eps_df, eps_df):
    acc_steps_to_recover = eps_df.query(
        "terminationCause == 1 & accRecovered"
    ).accStepsToRecover

    stats = {
        "R": comp_eps_df.globalSuccess.mean(),
        "\\mu_T": comp_eps_df.globalSteps.mean(),
        "\\sigma_T": comp_eps_df.globalSteps.std(),
        # u"min_global_steps":comp_eps_df.globalSteps.min(),
        # u"max_global_steps":comp_eps_df.globalSteps.max(),
        "\\mu_E": comp_eps_df.localEpisodesCount.mean(),
        "\\sigma_E": comp_eps_df.localEpisodesCount.std(),
        # u"min_local_episodes":comp_eps_df.localEpisodesCount.min(),
        # u"max_local_episodes":comp_eps_df.localEpisodesCount.max(),
        "\\mu_{T_{loc}}": eps_df.localSteps.mean(),
        "\\sigma_{T_{loc}}": eps_df.localSteps.std(),
        "\\mu_{T_{rec}}": acc_steps_to_recover.mean(),
        "\\sigma_{T_{rec}}": acc_steps_to_recover.std(),
        # u"acc_recovery_rate":eps_df.query("terminationCause == 1").accRecovered.mean(),
    }
    df = pd.DataFrame([stats])
    return df


def get_acc_violation_rate(eps_df: pd.DataFrame):
    return (eps_df.terminationCause == 1).mean()


def get_acc_violation_rate_per_skill(eps_df: pd.DataFrame, actions: Sequence[str]):
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


def get_local_steps_of_eps_violating_acc_per_acc(
    eps_df: pd.DataFrame,
    action_acc_tuples: Sequence[Tuple[str, str]],
    threshold=ACC_STEPS_TO_RECOVER_THRESHOLD,
):
    res = []
    for action, acc in action_acc_tuples:
        acc_df = eps_df.query(
            "action == @action & terminationCause == 1 & accName == @acc"
        )
        local_steps = acc_df.localSteps
        res.append(local_steps)

    return res


def get_acc_steps_to_recover_per_skill(
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
    acc_steps_to_recover = eps_df.query(
        "terminationCause == 1 & accRecovered & accStepsToRecover >= @threshold"
    ).accStepsToRecover

    return acc_steps_to_recover


def get_num_eps_per_skill(eps_df: pd.DataFrame, actions):
    # eps_per_action = eps_df.groupby(["action", "compositeEpisodeNumber"]).count().terminationCause.reset_index("compositeEpisodeNumber").groupby("action").terminationCause
    eps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").terminationCause.count()
        eps_per_action_list.append(avg_eps)
    return eps_per_action_list


def get_avg_num_eps_per_skill(eps_df: pd.DataFrame, actions):
    return [eps.mean() for eps in get_num_eps_per_skill(eps_df, actions)]


def get_total_steps_per_skill(eps_df: pd.DataFrame, actions):
    # summed up local steps (episode lengths) within a composite episode per action
    steps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").localSteps.sum()
        steps_per_action_list.append(avg_eps)
    return steps_per_action_list


def get_avg_total_steps_per_skill(eps_df: pd.DataFrame, actions):
    return [steps.mean() for steps in get_total_steps_per_skill(eps_df, actions)]


def get_local_steps_per_skill(eps_df: pd.DataFrame, actions):
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


def within_percentiles(data, percentiles):
    p1, p2 = percentiles
    v1, v2 = np.percentile(data, [p1, p2])
    print(v1, v2)
    return data[np.logical_and(data >= v1, data <= v2)]


"""https://stackoverflow.com/questions/15033511/compute-a-confidence-interval-from-sample-data"""


def condifence_interval(a, confidence=0.95):
    return st.t.interval(confidence, len(a) - 1, loc=np.mean(a), scale=st.sem(a))


"""https://stackoverflow.com/questions/15033511/compute-a-confidence-interval-from-sample-data"""


def mean_confidence_interval(data, confidence=0.95):
    a = 1.0 * np.array(data)
    n = len(a)
    m, se = np.mean(a), st.sem(a)
    h = se * st.t.ppf((1 + confidence) / 2.0, n - 1)
    return m, h, m - h, m + h


def acc_steps_recovered_sanity_check(df):
    grouped_df = df.groupby("compositeEpisodeNumber")
    for group in grouped_df.groups:
        comp_ep = grouped_df.get_group(group)
        steps = 0
        violated = {}
        for loc_ep in comp_ep.iloc:
            if loc_ep.action in violated:
                old_loc_ep, old_steps = violated.pop(loc_ep.action)
                steps_to_recover = steps - old_steps
                shouldbe = old_loc_ep.accStepsToRecover
                if abs(steps_to_recover - shouldbe) > 2 or not old_loc_ep.accRecovered:
                    print(group, shouldbe, steps_to_recover, old_loc_ep.accRecovered)

            steps += loc_ep.localSteps
            if loc_ep.terminationCause == 1:
                violated[loc_ep.action] = (loc_ep, steps)
        for loc_ep, _ in violated.values():
            assert not loc_ep.accRecovered


def acc_sanity_check(eps_df, action_acc_tuples):
    actions_data = list(eps_df.action.unique())
    accs_data = eps_df.query("terminationCause == 1").groupby("action").accName.unique()
    acc_dict_data = {key: list(value) for key, value in dict(accs_data).items()}
    action_acc_tuples_data = [
        (action, acc) for action in acc_dict_data for acc in acc_dict_data[action]
    ]
    for tuple in action_acc_tuples_data:
        if tuple not in action_acc_tuples:
            print("untracked acc:", tuple)
            action, acc = tuple
            weird_eps = eps_df.query(
                "terminationCause == 1 & action == @action & accName == @acc"
            )
            print(weird_eps)
            # assert len(weird_eps) == 0


def get_hpc_counts(df):
    hpc = df.query("terminationCause == 4")
    hpc_counts = hpc.groupby("postCondition").count().terminationCause
    return hpc_counts


def get_hpc_after_acc_violation_rate(df):
    hpc_counts = get_hpc_counts(df)
    hpc_count = sum(hpc_counts)
    hpc = df.query("terminationCause == 4")
    after_acc_violation_count = 0
    for hpc_ep in hpc.iloc:
        num = hpc_ep.compositeEpisodeNumber
        comp_ep = df.query("compositeEpisodeNumber == @num")
        prev_loc_num = hpc_ep.localEpisodeNumber - 1
        if prev_loc_num >= 0:
            prev_ep = comp_ep.query("localEpisodeNumber == @prev_loc_num").iloc[0]
            if prev_ep.terminationCause == 1:
                after_acc_violation_count += 1
    return after_acc_violation_count / hpc_count
