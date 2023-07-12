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
    plot_per_action,
    plot_per_acc,
    global_boxplot,
    boxplot_per_action,
    boxplot_per_acc,
)


run_id = "testRunId"

file_path_wcbf = f"evaluation/stats/{run_id}/statisticsWCBF_eps1e-2_newDyn.json"
file_path_wcbf2 = f"evaluation/stats/{run_id}/statisticsWCBF_eps1e-3_oldDyn.json"
eps_df_wcbf = load_repr1_to_eps(file_path_wcbf)

file_path_wocbf = f"evaluation/stats/{run_id}/statisticsWOCBF_eps1e-2_newDyn.json"
eps_df_wocbf = load_repr1_to_eps(file_path_wocbf)

file_paths = [file_path_wcbf, file_path_wcbf2, file_path_wocbf]
eps_dfs = [load_repr1_to_eps(file_path) for file_path in file_paths]

labels = ["WCBF", "WCBF2", "WOCBF"]

actions = eps_df_wocbf.action.unique()
accs = eps_df_wocbf.query("terminationCause == 1").groupby("action").accName.unique()
acc_dict = dict(accs)
action_acc_tuples = [(action, acc) for action in acc_dict for acc in acc_dict[action]]
# print("compositeEpisodeNumber:", eps_df.compositeEpisodeNumber.max() + 1)

comp_eps_dfs = [get_comp_eps_df(eps_df) for eps_df in eps_dfs]

stats = [gather_statistics(comp_eps_df) for comp_eps_df in comp_eps_dfs]
print(stats)

global_steps = [comp_eps_df.globalSteps for comp_eps_df in comp_eps_dfs]
global_boxplot(labels, global_steps, "steps", "Global Steps (Composite episode length)")

local_episodes_count = [comp_eps_df.localEpisodesCount for comp_eps_df in comp_eps_dfs]
global_boxplot(labels, local_episodes_count, "# episodes", "Local episodes count")


steps_to_recover = [get_acc_steps_to_recover(eps_df) for eps_df in eps_dfs]
print(steps_to_recover)
global_boxplot(labels, steps_to_recover, "steps", "Steps to recover")

steps_to_recover_per_action = [
    get_acc_steps_to_recover_per_action(eps_df, actions) for eps_df in eps_dfs
]
boxplot_per_action(
    actions, labels, steps_to_recover_per_action, "steps", "Steps to recover"
)

steps_to_recover_per_acc = [
    get_acc_steps_to_recover_per_acc(eps_df, action_acc_tuples) for eps_df in eps_dfs
]
boxplot_per_acc(
    action_acc_tuples, labels, steps_to_recover_per_acc, "steps", "Steps to recover"
)


# for action in actions:
#     print_action_summary(eps_df_wcbf, action)
#     print_action_summary(eps_df_wocbf, action)

acc_violation_rates = [get_acc_violation_rate(eps_df) for eps_df in eps_dfs]
print(acc_violation_rates)

acc_violation_rates_per_action = [
    get_acc_violation_rate_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_action(
    actions,
    labels,
    acc_violation_rates_per_action,
    "acc violation rate",
    "ACC violation rates",
)

acc_violation_rates_per_acc = [
    get_acc_violation_rate_per_acc(eps_df, action_acc_tuples) for eps_df in eps_dfs
]
plot_per_acc(
    action_acc_tuples,
    labels,
    acc_violation_rates_per_acc,
    "acc violation rate",
    "ACC violation rates",
)


avg_num_eps_per_action = [
    get_avg_num_eps_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_action(
    actions,
    labels,
    avg_num_eps_per_action,
    "# episodes",
    "Episodes per composite episode",
)

num_eps_per_action = [get_num_eps_per_action(eps_df, actions) for eps_df in eps_dfs]
boxplot_per_action(
    actions, labels, num_eps_per_action, "# episodes", "Episodes per composite episode"
)


avg_total_steps_per_action = [
    get_avg_total_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
plot_per_action(
    actions,
    labels,
    avg_total_steps_per_action,
    "Steps",
    "Total steps per composite episode",
)

total_steps_per_action = [
    get_total_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
boxplot_per_action(
    actions,
    labels,
    total_steps_per_action,
    "Steps",
    "Total steps per composite episode",
)


local_steps = [eps_df.localSteps for eps_df in eps_dfs]
global_boxplot(labels, local_steps, "steps", "Local Steps (Episode length)")

local_steps_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_dfs
]
boxplot_per_action(
    actions, labels, local_steps_per_action, "steps", "Local Steps (Episode length)"
)

# unnecessary
eps_reaching_pc_dfs = [eps_df.query("terminationCause == 0") for eps_df in eps_dfs]
eps_not_reaching_pc_dfs = [eps_df.query("terminationCause != 0") for eps_df in eps_dfs]

local_steps_reaching_pc = [eps_df.localSteps for eps_df in eps_reaching_pc_dfs]
local_steps_not_reaching_pc = [eps_df.localSteps for eps_df in eps_not_reaching_pc_dfs]
global_boxplot(
    labels,
    local_steps_not_reaching_pc,
    "steps",
    "Local Steps (Episode length) of episodes not reaching PC",
)

local_steps_reaching_pc_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_reaching_pc_dfs
]
local_steps_not_reaching_pc_per_action = [
    get_local_steps_per_action(eps_df, actions) for eps_df in eps_not_reaching_pc_dfs
]
boxplot_per_action(
    actions,
    labels,
    local_steps_not_reaching_pc_per_action,
    "steps",
    "Local Steps (Episode length) of episodes not reaching PC",
)

# compare local steps for episodes reaching PC and episodes not reaching PC
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc[i]
    not_reaching_pc = local_steps_not_reaching_pc[i]
    pc_labels = ["reaching pc", "not reaching pc"]
    data = [reaching_pc, not_reaching_pc]
    global_boxplot(pc_labels, data, "steps", f"Local Steps {label}")

# compare local steps for episodes reaching PC and episodes not reaching PCfor i in range(labels):
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc_per_action[i]
    not_reaching_pc = local_steps_not_reaching_pc_per_action[i]
    pc_labels = ["reaching pc", "not reaching pc"]
    data = [reaching_pc, not_reaching_pc]
    boxplot_per_action(
        actions,
        pc_labels,
        data,
        "steps",
        f"Local Steps {label}",
    )
