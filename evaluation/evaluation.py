import pandas as pd
from helpers import (
    boxplot_per_action,
    get_acc_violation_rate,
    get_avg_num_eps_per_action,
    get_comp_eps_df,
    get_num_eps_per_action,
    load_repr1_to_eps,
    Statistics,
    plot_per_action,
)


run_id = "testRunId"

file_path_wocbf = f"evaluation/stats/{run_id}/statisticsWOCBF.json"

action_termination_cause = [
    "PostConditionReached",
    "ACCViolated",
    "LocalReset",
    "GlobalReset",
]


eps_df = load_repr1_to_eps(file_path_wocbf)

actions = eps_df.action.unique()
accs = eps_df.query("terminationCause == 1").groupby("action").accName.unique()
print("compositeEpisodeNumber:", eps_df.compositeEpisodeNumber.max() + 1)

assert eps_df.query("terminationCause == 1")[
    eps_df.query("terminationCause == 1").accName.isnull()
].empty  # otherwise there exist acc violations that are not properly tracked


comp_eps_df = get_comp_eps_df(eps_df)
print(comp_eps_df)
stats = Statistics(
    comp_eps_df.globalSuccess.mean(),
    comp_eps_df.globalSteps.min(),
    comp_eps_df.globalSteps.max(),
    comp_eps_df.globalSteps.mean(),
    comp_eps_df.globalSteps.std(),
)
print(stats)


# for action in actions:
#     print_action_summary(eps_df, action)


file_path2 = f"evaluation/stats/{run_id}/statisticsWCBF.json"
eps_df2 = load_repr1_to_eps(file_path2)


acc_violation_rates = {
    "WOCBF": get_acc_violation_rate(eps_df, actions),
    "WCBF": get_acc_violation_rate(eps_df2, actions),
}


ylabel = "acc violation rate"
title = "ACC violation rates"

plot_per_action(actions, acc_violation_rates, ylabel, title)


eps_per_action = {
    "WOCBF": get_avg_num_eps_per_action(eps_df, actions),
    "WCBF": get_avg_num_eps_per_action(eps_df2, actions),
}


plot_per_action(actions, eps_per_action, "Episodes", "Episodes per action")


def get_avg_total_steps_per_action(eps_df: pd.DataFrame, actions):
    steps_per_action_list = []
    for action in actions:
        action_df = eps_df.query("action == @action")
        avg_eps = action_df.groupby("compositeEpisodeNumber").localSteps.sum().mean()
        steps_per_action_list.append(avg_eps)
    return steps_per_action_list


steps_per_action = {
    "WOCBF": get_avg_total_steps_per_action(eps_df, actions),
    "WCBF": get_avg_total_steps_per_action(eps_df2, actions),
}

plot_per_action(actions, steps_per_action, "Steps", "Steps per action")


eps_data_per_action = {
    "WOCBF": get_num_eps_per_action(eps_df, actions),
    "WCBF": get_num_eps_per_action(eps_df2, actions),
}


ylabel = "# episodes"
title = "Episodes per composite episode"

boxplot_per_action(actions, eps_data_per_action, ylabel, title)
