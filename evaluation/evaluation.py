from helpers import (
    load_repr1_to_eps,
    gather_statistics,
    print_action_summary,
    get_comp_eps_df,
    get_acc_violation_rate_per_action,
    get_num_eps_per_action,
    get_avg_num_eps_per_action,
    get_total_steps_per_action,
    get_avg_total_steps_per_action,
    get_acc_steps_to_recover,
    get_acc_steps_to_recover_per_action,
    get_local_steps_per_action,
    global_boxplot,
    plot_per_action,
    boxplot_per_action,
)


run_id = "testRunId"

file_path_wcbf = f"evaluation/stats/{run_id}/statisticsWCBF.json"
eps_df_wcbf = load_repr1_to_eps(file_path_wcbf)

file_path_wocbf = f"evaluation/stats/{run_id}/statisticsWOCBF.json"
eps_df_wocbf = load_repr1_to_eps(file_path_wocbf)

actions = eps_df_wcbf.action.unique()
accs = eps_df_wcbf.query("terminationCause == 1").groupby("action").accName.unique()
# print("compositeEpisodeNumber:", eps_df.compositeEpisodeNumber.max() + 1)

comp_eps_df_wcbf = get_comp_eps_df(eps_df_wcbf)
comp_eps_df_wocbf = get_comp_eps_df(eps_df_wocbf)

stats_wcbf = gather_statistics(comp_eps_df_wcbf)
stats_wocbf = gather_statistics(comp_eps_df_wocbf)

print(stats_wcbf)
print(stats_wocbf)

global_steps = [comp_eps_df_wcbf.globalSteps, comp_eps_df_wocbf.globalSteps]
global_boxplot(["WCBF", "WOCBF"], global_steps, "steps", "Global Steps")

local_episodes_count = [
    comp_eps_df_wcbf.localEpisodesCount,
    comp_eps_df_wocbf.localEpisodesCount,
]
global_boxplot(
    ["WCBF", "WOCBF"], local_episodes_count, "# episodes", "Local episodes count"
)


steps_to_recover = [
    get_acc_steps_to_recover(eps_df_wcbf),
    get_acc_steps_to_recover(eps_df_wocbf),
]
global_boxplot(["WCBF", "WOCBF"], steps_to_recover, "steps", "Steps to recover")

steps_to_recover_per_action = {
    "WCBF": get_acc_steps_to_recover_per_action(eps_df_wcbf, actions),
    "WOCBF": get_acc_steps_to_recover_per_action(eps_df_wocbf, actions),
}
boxplot_per_action(actions, steps_to_recover_per_action, "steps", "Steps to recover")


# for action in actions:
#     print_action_summary(eps_df_wcbf, action)
#     print_action_summary(eps_df_wocbf, action)


acc_violation_rates_per_action = {
    "WCBF": get_acc_violation_rate_per_action(eps_df_wcbf, actions),
    "WOCBF": get_acc_violation_rate_per_action(eps_df_wocbf, actions),
}
plot_per_action(
    actions, acc_violation_rates_per_action, "acc violation rate", "ACC violation rates"
)


avg_eps_per_action = {
    "WCBF": get_avg_num_eps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_avg_num_eps_per_action(eps_df_wocbf, actions),
}
plot_per_action(
    actions, avg_eps_per_action, "# episodes", "Episodes per composite episode"
)

eps_data_per_action = {
    "WCBF": get_num_eps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_num_eps_per_action(eps_df_wocbf, actions),
}
boxplot_per_action(
    actions, eps_data_per_action, "# episodes", "Episodes per composite episode"
)


avg_steps_per_action = {
    "WCBF": get_avg_total_steps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_avg_total_steps_per_action(eps_df_wocbf, actions),
}
plot_per_action(
    actions, avg_steps_per_action, "Steps", "Total steps per composite episode"
)

steps_per_action = {
    "WCBF": get_total_steps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_total_steps_per_action(eps_df_wocbf, actions),
}
boxplot_per_action(
    actions, steps_per_action, "Steps", "Total steps per composite episode"
)


local_steps = [
    eps_df_wcbf.localSteps,
    eps_df_wocbf.localSteps,
]
global_boxplot(["WCBF", "WOCBF"], local_steps, "steps", "Local Steps (Episode length)")

local_steps_per_action = {
    "WCBF": get_local_steps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_local_steps_per_action(eps_df_wocbf, actions),
}
boxplot_per_action(
    actions, local_steps_per_action, "steps", "Local Steps (Episode length)"
)
