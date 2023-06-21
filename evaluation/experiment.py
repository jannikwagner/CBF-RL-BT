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
