from helpers import (
    load_repr1_to_eps,
    gather_statistics,
    get_comp_eps_df,
    get_termination_cause_rates,
    skill_termination_causes,
    acc_sanity_check,
    acc_steps_recovered_sanity_check,
)

import seaborn as sns
import pandas as pd

NUM_EPISODES = 5000
store_folder = "test"

run_id = "testRunId"

w_f_s = "env5.wcbf.fixedbridge.safeplace"
wo_f_s = "env5.wocbf.fixedbridge.safeplace"
w_f_ns = "env5.wcbf.fixedbridge.notsafeplace"
wo_f_ns = "env5.wocbf.fixedbridge.notsafeplace"
w_nf_s = "env5.wcbf.notfixedbridge.safeplace"
wo_nf_s = "env5.wocbf.notfixedbridge.safeplace"

file_names = [w_f_s, wo_f_s, w_f_ns, wo_f_ns, w_nf_s, wo_nf_s]

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

labels = file_names

skills = [
    "MoveToT1",
    "MoveUp",
    "MoveUp2",
    "MoveToB1",
    "MoveToT2",
    "MoveToBridge",
    "MoveOverBridge",
    "MoveToB2",
]
acc_dict = {
    "MoveToT1": [],
    "MoveUp": [],
    "MoveUp2": [],
    "MoveToB1": ["Up"],
    "MoveToT2": ["B1"],
    "MoveToBridge": ["B1", "Up"],
    "MoveOverBridge": ["OnBridge"],
    "MoveToB2": ["PastBridge"],
}
skill_acc_tuples = [(skill, acc) for skill in acc_dict for acc in acc_dict[skill]]


for df in eps_dfs:
    acc_steps_recovered_sanity_check(df)
    acc_sanity_check(df, skill_acc_tuples)

comp_eps_dfs = [get_comp_eps_df(eps_df) for eps_df in eps_dfs]

stats = [
    gather_statistics(comp_eps_df, eps_df)
    for comp_eps_df, eps_df in zip(comp_eps_dfs, eps_dfs)
]
print("global_stats:")
stats_df = pd.concat(stats)
stats_df.index = labels
# print(stats_df.to_dict())
print(stats_df.to_latex())

skill_termination_cause_df = pd.DataFrame(
    [
        dict(zip(skill_termination_causes, get_termination_cause_rates(df)))
        for df in eps_dfs
    ]
)
skill_termination_cause_df.index = labels
# print(skill_termination_cause_df.to_dict())
print(skill_termination_cause_df.to_latex())
