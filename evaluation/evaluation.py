from helpers import (
    load_repr1_to_eps,
    gather_statistics,
    print_skill_summary,
    get_comp_eps_df,
    get_acc_violation_rate,
    get_acc_violation_rate_per_skill,
    get_acc_violation_rate_per_acc,
    get_num_eps_per_skill,
    get_avg_num_eps_per_skill,
    get_total_steps_per_skill,
    get_avg_total_steps_per_skill,
    get_acc_steps_to_recover,
    get_acc_steps_to_recover_per_skill,
    get_acc_steps_to_recover_per_acc,
    get_local_steps_per_skill,
    get_termination_cause_rates,
    plot_per_acc,
    plot_per_group,
    global_plot,
    bars_per_group,
    bars_per_acc,
    get_local_steps_of_eps_violating_acc_per_acc,
    SkillTerminationCause,
    skill_termination_causes,
    acc_sanity_check,
    acc_steps_recovered_sanity_check,
)

import seaborn as sns
import pandas as pd

NUM_EPISODES = 5000

skills = [
    "MTrigger1",
    "MUp",
    "MUp2",
    "MButton1",
    "MTrigger2",
    "MTBridge",
    "MOBridge",
    "MButton2",
]
old_skills = [
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
    "MTrigger1": [],
    "MUp": [],
    "MUp2": [],
    "MButton1": ["Up"],
    "MTrigger2": ["Button1"],
    "MTBridge": ["Button1", "Up2"],
    "MOBridge": ["OnBridge"],
    "MButton2": ["PastBridge"],
}
old_acc_dict = {
    "MoveToT1": [],
    "MoveUp": [],
    "MoveUp2": [],
    "MoveToB1": ["Up"],
    "MoveToT2": ["B1"],
    "MoveToBridge": ["B1", "Up"],
    "MoveOverBridge": ["OnBridge"],
    "MoveToB2": ["PastBridge"],
}


def get_skill_acc_tuples(acc_dict):
    skill_acc_tuples = [(skill, acc) for skill in acc_dict for acc in acc_dict[skill]]
    return skill_acc_tuples


skill_acc_tuples = get_skill_acc_tuples(acc_dict)
old_skill_acc_tuples = get_skill_acc_tuples(old_acc_dict)


def rename_skills_and_accs(df, skill_acc_tuples, old_skill_acc_tuples):
    for (s1, a1), (s2, a2) in zip(old_skill_acc_tuples, skill_acc_tuples):
        print((s1, a1), (s2, a2))
        index = df.query("action == @s1").index
        df.loc[index, ["action"]] = s2
        index = df.query("action == @s2 & terminationCause == 1 & accName == @a1").index
        df.loc[index, "accName"] = a2


run_id = "testRunId"

w_f_s = "env5.wcbf.fixedbridge.safeplace"
wo_f_s = "env5.wocbf.fixedbridge.safeplace"
w_f_ns = "env5.wcbf.fixedbridge.notsafeplace"
wo_f_ns = "env5.wocbf.fixedbridge.notsafeplace"
w_nf_s = "env5.wcbf.notfixedbridge.safeplace"
wo_nf_s = "env5.wocbf.notfixedbridge.safeplace"

f_s = w_f_s, wo_f_s
f_ns = w_f_ns, wo_f_ns
nf_s = w_nf_s, wo_nf_s

store_folder = "fixedbridge.notsafeplace"

file_names = f_ns

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


for df in eps_dfs:
    rename_skills_and_accs(df, skill_acc_tuples, old_skill_acc_tuples)
    acc_steps_recovered_sanity_check(df)
    acc_sanity_check(df, skill_acc_tuples)

comp_eps_dfs = [get_comp_eps_df(eps_df) for eps_df in eps_dfs]


global_steps = [comp_eps_df.globalSteps for comp_eps_df in comp_eps_dfs]
global_plot(
    labels, global_steps, "steps", "Composite Episode Length", store_folder=store_folder
)

local_episodes_count = [comp_eps_df.localEpisodesCount for comp_eps_df in comp_eps_dfs]
# print("local episode counts unique", [x.unique() for x in local_episodes_count])
global_plot(
    labels,
    local_episodes_count,
    "episodes",
    "Local Episodes per Composite Episode",
    store_folder=store_folder,
)

termination_cause_rates = [get_termination_cause_rates(df) for df in eps_dfs]
bars_per_group(
    skill_termination_causes,
    labels,
    termination_cause_rates,
    "termination cause rate",
    "Termination Cause Rates",
    store_folder=store_folder,
)

for skill in skills:
    skill_dfs = [df.query("action == @skill") for df in eps_dfs]
    termination_cause_rates = [get_termination_cause_rates(df) for df in skill_dfs]
    bars_per_group(
        skill_termination_causes,
        labels,
        termination_cause_rates,
        "termination cause rate",
        f"Termination Cause Rates for Skill {skill}",
        store_folder=store_folder,
    )


steps_to_recover = [get_acc_steps_to_recover(eps_df) for eps_df in eps_dfs]
# print("steps_to_recover:", steps_to_recover)
global_plot(
    labels, steps_to_recover, "steps", "Steps to Recover", store_folder=store_folder
)

steps_to_recover_per_skill = [
    get_acc_steps_to_recover_per_skill(eps_df, skills) for eps_df in eps_dfs
]
plot_per_group(
    skills,
    labels,
    steps_to_recover_per_skill,
    "steps",
    "Steps to Recover grouped by Skill",
    store_folder=store_folder,
)

steps_to_recover_per_acc = [
    get_acc_steps_to_recover_per_acc(eps_df, skill_acc_tuples) for eps_df in eps_dfs
]
plot_per_acc(
    skill_acc_tuples,
    labels,
    steps_to_recover_per_acc,
    "steps",
    "Steps to Recover grouped by ACC",
    store_folder=store_folder,
)

acc_violation_rates = [get_acc_violation_rate(eps_df) for eps_df in eps_dfs]
# print("acc_violation_rates:", acc_violation_rates)

acc_violation_rates_per_skill = [
    get_acc_violation_rate_per_skill(eps_df, skills) for eps_df in eps_dfs
]
bars_per_group(
    skills,
    labels,
    acc_violation_rates_per_skill,
    "ACC violation rate",
    "ACC Violation Rates grouped by Skill",
    store_folder=store_folder,
)

acc_violation_rates_per_acc = [
    get_acc_violation_rate_per_acc(eps_df, skill_acc_tuples) for eps_df in eps_dfs
]
bars_per_acc(
    skill_acc_tuples,
    labels,
    acc_violation_rates_per_acc,
    "ACC violation rate",
    "ACC Violation Rates grouped by ACC",
    store_folder=store_folder,
)


avg_num_eps_per_skill = [
    get_avg_num_eps_per_skill(eps_df, skills) for eps_df in eps_dfs
]
bars_per_group(
    skills,
    labels,
    avg_num_eps_per_skill,
    "episodes",
    "Average Local Episodes per Composite Episode grouped by Skill",
    store_folder=store_folder,
)

num_eps_per_skill = [get_num_eps_per_skill(eps_df, skills) for eps_df in eps_dfs]
plot_per_group(
    skills,
    labels,
    num_eps_per_skill,
    "episodes",
    "Local Episodes per Composite Episode grouped by Skill",
    store_folder=store_folder,
)

avg_total_steps_per_skill = [
    get_avg_total_steps_per_skill(eps_df, skills) for eps_df in eps_dfs
]
bars_per_group(
    skills,
    labels,
    avg_total_steps_per_skill,
    "steps",
    "Average Total Steps per Composite Episode grouped by Skill",
    store_folder=store_folder,
)

total_steps_per_skill = [
    get_total_steps_per_skill(eps_df, skills) for eps_df in eps_dfs
]
plot_per_group(
    skills,
    labels,
    total_steps_per_skill,
    "steps",
    "Total Steps per Composite Episode grouped by Skill",
    store_folder=store_folder,
)

local_steps = [eps_df.localSteps for eps_df in eps_dfs]
global_plot(
    labels, local_steps, "steps", "Local Episode Length", store_folder=store_folder
)

local_steps_per_skill = [
    get_local_steps_per_skill(eps_df, skills) for eps_df in eps_dfs
]
plot_per_group(
    skills,
    labels,
    local_steps_per_skill,
    "steps",
    "Local Episode Length grouped by Skill",
    store_folder=store_folder,
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
    store_folder=store_folder,
)

local_steps_reaching_pc_per_skill = [
    get_local_steps_per_skill(eps_df, skills) for eps_df in eps_reaching_pc_dfs
]
local_steps_violating_acc_per_skill = [
    get_local_steps_per_skill(eps_df, skills) for eps_df in eps_violating_acc_dfs
]
plot_per_group(
    skills,
    labels,
    local_steps_violating_acc_per_skill,
    "steps",
    "Length of Local Episodes violating ACC grouped by Skill",
    store_folder=store_folder,
)

local_steps_violating_acc_per_acc = [
    get_local_steps_of_eps_violating_acc_per_acc(eps_df, skill_acc_tuples)
    for eps_df in eps_dfs
]
plot_per_acc(
    skill_acc_tuples,
    labels,
    local_steps_violating_acc_per_acc,
    "steps",
    "Length of Local Episodes violating ACC grouped by ACC",
    store_folder=store_folder,
)

# compare local steps for episodes reaching PC and episodes not reaching PC
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc[i]
    violating_acc = local_steps_violating_acc[i]
    pc_labels = ["reaching pc", "violating acc"]
    data = [reaching_pc, violating_acc]
    global_plot(
        pc_labels,
        data,
        "steps",
        f"Local Episode Length - {label}",
        store_folder=store_folder,
    )

# compare local steps for episodes reaching PC and episodes not reaching PCfor i in range(labels):
for i in range(len(labels)):
    label = labels[i]
    reaching_pc = local_steps_reaching_pc_per_skill[i]
    violating_acc = local_steps_violating_acc_per_skill[i]
    pc_labels = ["reaching pc", "violating acc"]
    data = [reaching_pc, violating_acc]
    plot_per_group(
        skills,
        pc_labels,
        data,
        "steps",
        f"Local Episode Length grouped by Skill - {label}",
        store_folder=store_folder,
    )
