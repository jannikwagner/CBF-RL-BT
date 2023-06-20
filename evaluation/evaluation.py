from helpers import (
    load_repr1_to_eps,
    gather_statistics,
    print_action_summary,
    get_acc_violation_rate,
    get_avg_num_eps_per_action,
    get_comp_eps_df,
    get_num_eps_per_action,
    get_avg_total_steps_per_action,
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

global_steps = {
    "Global Steps": [comp_eps_df_wcbf.globalSteps, comp_eps_df_wocbf.globalSteps]
}
boxplot_per_action(["WCBF", "WOCBF"], global_steps, "Global Steps", "Global Steps")


# for action in actions:
#     print_action_summary(eps_df_wcbf, action)
#     print_action_summary(eps_df_wocbf, action)


acc_violation_rates = {
    "WCBF": get_acc_violation_rate(eps_df_wcbf, actions),
    "WOCBF": get_acc_violation_rate(eps_df_wocbf, actions),
}

plot_per_action(
    actions, acc_violation_rates, "acc violation rate", "ACC violation rates"
)


avg_eps_per_action = {
    "WCBF": get_avg_num_eps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_avg_num_eps_per_action(eps_df_wocbf, actions),
}

plot_per_action(actions, avg_eps_per_action, "Episodes", "Episodes per action")


eps_data_per_action = {
    "WCBF": get_num_eps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_num_eps_per_action(eps_df_wocbf, actions),
}

boxplot_per_action(
    actions, eps_data_per_action, "# episodes", "Episodes per composite episode"
)


steps_per_action = {
    "WCBF": get_avg_total_steps_per_action(eps_df_wcbf, actions),
    "WOCBF": get_avg_total_steps_per_action(eps_df_wocbf, actions),
}

plot_per_action(actions, steps_per_action, "Steps", "Steps per action")
