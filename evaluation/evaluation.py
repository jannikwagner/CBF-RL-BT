from helpers import (
    load_repr1_to_eps,
    gather_statistics,
    print_action_summary,
    get_comp_eps_df,
    get_acc_violation_rate,
    get_acc_violation_rate_per_action,
    get_acc_violation_rate_per_acc,
    get_num_eps_per_action,
    get_avg_num_eps_per_action,
    get_total_steps_per_action,
    get_avg_total_steps_per_action,
    get_acc_steps_to_recover,
    get_acc_steps_to_recover_per_action,
    get_acc_steps_to_recover_per_acc,
    get_local_steps_per_action,
    get_termination_cause_rates,
    plot_per_acc,
    plot_per_group,
    global_plot,
    bars_per_group,
    bars_per_acc,
    ActionTerminationCause,
    action_termination_causes,
)

import seaborn as sns
import pandas as pd

NUM_EPISODES = 5000
show = False

run_id = "testRunId"

file_name_wcbf = "env5.wcbf.fixedbridge.safeplace"
file_name_wocbf = "env5.wocbf.fixedbridge.safeplace"
file_names = [file_name_wcbf, file_name_wocbf]

file_paths = [f"evaluation/stats/{run_id}/{file_name}.json" for file_name in file_names]

eps_dfs = [load_repr1_to_eps(file_path) for file_path in file_paths]
for df in eps_dfs:
    print("max compositeEpisodeNumber:", df.compositeEpisodeNumber.max())
    assert (df.compositeEpisodeNumber.max()) >= NUM_EPISODES - 1
eps_dfs = [df.query("compositeEpisodeNumber < @NUM_EPISODES") for df in eps_dfs]
for df in eps_dfs:
    print("max compositeEpisodeNumber:", df.compositeEpisodeNumber.max())
    assert (df.compositeEpisodeNumber.max()) >= NUM_EPISODES - 1
eps_df_wocbf = eps_dfs[1]

labels = ["wcbf", "wocbf"]

actions = eps_df_wocbf.action.unique()
accs = eps_df_wocbf.query("terminationCause == 1").groupby("action").accName.unique()
acc_dict = dict(accs)
action_acc_tuples = [(action, acc) for action in acc_dict for acc in acc_dict[action]]

comp_eps_dfs = [get_comp_eps_df(eps_df) for eps_df in eps_dfs]

stats = [
    gather_statistics(comp_eps_df, eps_df)
    for comp_eps_df, eps_df in zip(comp_eps_dfs, eps_dfs)
]
print("global_stats:")
stats_df = pd.concat(stats)
stats_df.index = labels
print(stats_df.to_dict())
print(stats_df.to_latex())

action_termination_cause_df = pd.DataFrame(
    [
        dict(zip(action_termination_causes, get_termination_cause_rates(df)))
        for df in eps_dfs
    ]
)
action_termination_cause_df.index = labels
print(action_termination_cause_df.to_dict())
print(action_termination_cause_df.to_latex())
global_steps = [comp_eps_df.globalSteps for comp_eps_df in comp_eps_dfs]
global_plot(labels, global_steps, "steps", "Composite Episode Length", show=show)

local_episodes_count = [comp_eps_df.localEpisodesCount for comp_eps_df in comp_eps_dfs]
global_plot(
    labels,
    local_episodes_count,
    "episodes",
    "Local Episodes per Composite Episode",
    show=show,
)

termination_cause_rates = [get_termination_cause_rates(df) for df in eps_dfs]
bars_per_group(
    action_termination_causes,
    labels,
    termination_cause_rates,
    "termination cause rate",
    "Termination Cause Rates",
    show=show,
)

for action in actions:
    action_dfs = [df.query("action == @action") for df in eps_dfs]
    termination_cause_rates = [get_termination_cause_rates(df) for df in action_dfs]
    bars_per_group(
        action_termination_causes,
        labels,
        termination_cause_rates,
        "termination cause rate",
        f"Termination Cause Rates for Action {action}",
        show=show,
    )


steps_to_recover = [get_acc_steps_to_recover(eps_df) for eps_df in eps_dfs]
print("steps_to_recover:", steps_to_recover)
global_plot(labels, steps_to_recover, "steps", "Steps to Recover", show=show)

steps_to_recover_per_action = [
    get_acc_steps_to_recover_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_group(
    actions,
    labels,
    steps_to_recover_per_action,
    "steps",
    "Steps to Recover grouped by Action",
    show=show,
)

steps_to_recover_per_acc = [
    get_acc_steps_to_recover_per_acc(eps_df, action_acc_tuples) for eps_df in eps_dfs
]
plot_per_acc(
    action_acc_tuples,
    labels,
    steps_to_recover_per_acc,
    "steps",
    "Steps to Recover grouped by ACC",
    show=show,
)

acc_violation_rates = [get_acc_violation_rate(eps_df) for eps_df in eps_dfs]
print("acc_violation_rates:", acc_violation_rates)

acc_violation_rates_per_action = [
    get_acc_violation_rate_per_action(eps_df, actions) for eps_df in eps_dfs
]
bars_per_group(
    actions,
    labels,
    acc_violation_rates_per_action,
    "ACC violation rate",
    "ACC Violation Rates grouped by Action",
    show=show,
)

acc_violation_rates_per_acc = [
    get_acc_violation_rate_per_acc(eps_df, action_acc_tuples) for eps_df in eps_dfs
]
bars_per_acc(
    action_acc_tuples,
    labels,
    acc_violation_rates_per_acc,
    "ACC violation rate",
    "ACC Violation Rates grouped by ACC",
    show=show,
)


avg_num_eps_per_action = [
    get_avg_num_eps_per_action(eps_df, actions) for eps_df in eps_dfs
]
bars_per_group(
    actions,
    labels,
    avg_num_eps_per_action,
    "episodes",
    "Average Local Episodes per Composite Episode grouped by Action",
    show=show,
)

num_eps_per_action = [get_num_eps_per_action(eps_df, actions) for eps_df in eps_dfs]
plot_per_group(
    actions,
    labels,
    num_eps_per_action,
    "episodes",
    "Local Episodes per Composite Episode grouped by Action",
    show=show,
)

avg_total_steps_per_action = [
    get_avg_total_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
bars_per_group(
    actions,
    labels,
    avg_total_steps_per_action,
    "steps",
    "Average Total Steps per Composite Episode grouped by Action",
    show=show,
)

total_steps_per_action = [
    get_total_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_group(
    actions,
    labels,
    total_steps_per_action,
    "steps",
    "Total Steps per Composite Episode grouped by Action",
    show=show,
)

local_steps = [eps_df.localSteps for eps_df in eps_dfs]
global_plot(labels, local_steps, "steps", "Local Episode Length", show=show)

local_steps_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_group(
    actions,
    labels,
    local_steps_per_action,
    "steps",
    "Local Episode Length grouped by Action",
    show=show,
)

eps_reaching_pc_dfs = [eps_df.query("terminationCause == 0") for eps_df in eps_dfs]
eps_not_reaching_pc_dfs = [eps_df.query("terminationCause != 0") for eps_df in eps_dfs]
eps_violating_acc_dfs = [eps_df.query("terminationCause == 1") for eps_df in eps_dfs]

local_steps_reaching_pc = [eps_df.localSteps for eps_df in eps_reaching_pc_dfs]
local_steps_violating_acc = [eps_df.localSteps for eps_df in eps_violating_acc_dfs]
global_plot(
    labels,
    local_steps_violating_acc,
    "steps",
    "Length of Local Episodes Violating ACCs",
    show=show,
)

local_steps_reaching_pc_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_reaching_pc_dfs
]
local_steps_violating_acc_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_violating_acc_dfs
]
plot_per_group(
    actions,
    labels,
    local_steps_violating_acc_per_action,
    "steps",
    "Length of Local Episodes violating ACC grouped by Action",
    show=show,
)

# compare local steps for episodes reaching PC and episodes not reaching PC
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc[i]
    violating_acc = local_steps_violating_acc[i]
    pc_labels = ["reaching pc", "violating acc"]
    data = [reaching_pc, violating_acc]
    global_plot(pc_labels, data, "steps", f"Local Episode Length - {label}", show=show)
    # global_hist(pc_labels, data, "steps", f"Local Episode Length - {label}", show=show)

# compare local steps for episodes reaching PC and episodes not reaching PCfor i in range(labels):
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc_per_action[i]
    violating_acc = local_steps_violating_acc_per_action[i]
    pc_labels = ["reaching pc", "violating acc"]
    data = [reaching_pc, violating_acc]
    plot_per_group(
        actions,
        pc_labels,
        data,
        "steps",
        f"Local Episode Length grouped by Action - {label}",
        show=show,
    )
