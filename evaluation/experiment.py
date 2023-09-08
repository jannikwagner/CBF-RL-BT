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
    global_boxplot,
    # global_hist,
    global_violinplot,
    violinplot_per_group,
    bars_per_group,
    bars_per_acc,
    boxplot_per_group,
    boxplot_per_acc,
    ActionTerminationCause,
    action_termination_causes,
    get_local_steps_of_eps_violating_acc_per_acc,
    plot_per_acc,
    acc_steps_recovered_sanity_check,
    global_plot,
    get_hpc_counts,
    get_hpc_after_acc_violation_rate,
)

import seaborn as sns
import pandas as pd

NUM_EPISODES = 5000
store_folder = "test"

run_id = "testRunId"

file_name_wcbf = "env5.wcbf.fixedbridge.notsafeplace"
file_name_wocbf = "env5.wocbf.fixedbridge.notsafeplace"
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

labels = ["WCBF", "WOCBF"]

actions = eps_df_wocbf.action.unique()
accs = eps_df_wocbf.query("terminationCause == 1").groupby("action").accName.unique()
acc_dict = dict(accs)
action_acc_tuples = [(action, acc) for action in acc_dict for acc in acc_dict[action]]

comp_eps_dfs = [get_comp_eps_df(eps_df) for eps_df in eps_dfs]

df = eps_dfs[1]


hpc_counts = get_hpc_counts(df)

after_acc_violation_rate = get_hpc_after_acc_violation_rate(df)

print(dict(hpc_counts))
print(hpc_counts)
print(after_acc_violation_rate)

import matplotlib.pyplot as plt
import numpy as np

d1 = np.random.randn(1000) - 1
d2 = np.random.randn(100000) / 2
plt.violinplot([d1, d2])
plt.show()
