from plotting import (
    get_scalar_dataframe_from_tb_files,
    get_hist_dataframe_from_tb_files,
    get_reward_series,
    plot_histogram,
    plot_multi_series,
    plot_reward_series,
)
import numpy as np
import os
import seaborn as sns
import pandas as pd

font_family = "PT Mono"
background_color = "#F8F1F1"
text_color = "#040303"

sns.set_style(
    {
        "axes.facecolor": background_color,
        "figure.facecolor": background_color,
        "font.family": font_family,
        "text.color": text_color,
    }
)


def get_scalar_data_all_skills(run_path):
    run_logs = "run_logs"
    data = {}
    for name in os.listdir(run_path):
        if name == run_logs:
            continue
        dir_name = os.path.join(run_path, name)
        if os.path.isdir(dir_name):
            df = get_scalar_dataframe_from_tb_files([dir_name])
            data[name] = df
    return data


def get_scalar_data(results_path, run_ids, skills, labels=None):
    data = {}
    i = 0
    for run_id in run_ids:
        for skill in skills:
            skill_path = os.path.join(results_path, run_id, skill)
            name = (
                skill
                if len(run_ids) == 1
                else (run_id if len(skills) == 1 else f"{run_id}/{skill}")
            )
            if labels is not None:
                name = labels[i]
                i += 1
            df = get_scalar_dataframe_from_tb_files([skill_path])
            data[name] = df
    return data


def print_total_steps(results_path, skills, get_scalar_data, done_run_ids):
    steps = {}
    for run_id in done_run_ids:
        data = get_scalar_data(results_path, [run_id], skills)
        temp = {label: min(5000000, d["Timestep"].max()) for label, d in data.items()}
        temp["sum"] = sum(temp.values())
        steps[run_id] = temp

    steps_df = pd.DataFrame(steps).transpose()
    print(steps)
    print(steps_df)
    print(steps_df.to_latex())


def time_series_all_skills(results_path, run_id, skills, figsize=(12, 7), path=""):
    data = get_scalar_data(results_path, [run_id], skills)
    plot_multi_series(data, figsize, title=run_id, store=os.path.join(path, run_id))


def time_series_one_skill(results_path, run_ids, skill, title, labels):
    data = get_scalar_data(results_path, run_ids, [skill], labels)
    plot_multi_series(data, (5, 3), title=title, store=title)


results_path = "results"
w_f_s = "env5.wcbf.fixedbridge.safeplace"
wo_f_s = "env5.wocbf.fixedbridge.safeplace"
w_f_ns = "env5.wcbf.fixedbridge.notsafeplace"
wo_f_ns = "env5.wocbf.fixedbridge.notsafeplace"
w_nf_s = "env5.wcbf.notfixedbridge.safeplace"
wo_nf_s = "env5.wocbf.notfixedbridge.safeplace"
skills = [
    "MoveToTrigger1",
    "MoveToButton2",
    "MoveUp",
    "MoveToBridge",
    "MoveToTrigger2",
    "MoveToButton1",
    "MoveOverBridge",
]

done_run_ids = [w_f_s, wo_f_s, w_f_ns, wo_f_ns, w_nf_s, wo_nf_s]


for run_id in done_run_ids:
    time_series_all_skills(results_path, run_id, skills, (7, 5), "mean_small")

print_total_steps(results_path, skills, get_scalar_data, done_run_ids)

for run_id in done_run_ids:
    time_series_all_skills(results_path, run_id, skills, (12, 7))

f_s = (w_f_s, wo_f_s)
labels = ("wcbf", "wocbf")
for skill in skills:
    time_series_one_skill(
        results_path, f_s, skill, f"fixedbridge.safeplace/{skill}", labels
    )

f_ns = (w_f_ns, wo_f_ns)
labels = ("wcbf", "wocbf")
for skill in skills:
    time_series_one_skill(
        results_path, f_ns, skill, f"fixedbridge.notsafeplace/{skill}", labels
    )

nf_s = (w_nf_s, wo_nf_s)
labels = ("wcbf", "wocbf")
for skill in skills:
    time_series_one_skill(
        results_path, nf_s, skill, f"notfixedbridge.safeplace/{skill}", labels
    )

w_f = (w_f_s, w_f_ns)
labels = ("safeplace", "notsafeplace")
for skill in skills:
    time_series_one_skill(results_path, w_f, skill, f"wcbf.fixedbridge/{skill}", labels)

w_s = (w_f_s, w_nf_s)
labels = ("fixedbridge", "notfixedbridge")
for skill in skills:
    time_series_one_skill(results_path, w_s, skill, f"wcbf.safeplace/{skill}", labels)
