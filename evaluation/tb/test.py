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


results_path = "results"
w_f_s = "env5.wcbf.fixedbridge.safeplace"
wo_f_s = "env5.wocbf.fixedbridge.safeplace"
w_f_ns = "env5.wcbf.fixedbridge.notsafeplace"
wo_f_ns = "env5.wocbf.fixedbridge.notsafeplace"
w_nf_s = "env5.wcbf.notfixedbridge.safeplace"
wo_nf_s = "env5.wocbf.notfixedbridge.safeplace"
behaviors = [
    "MoveToTrigger1",
    "MoveToButton2",
    "MoveUp",
    "MoveToBridge",
    "MoveToTrigger2",
    "MoveToButton1",
    "MoveOverBridge",
]


def get_scalar_data_all_behaviors(run_path):
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


def get_scalar_data(results_path, run_ids, behaviors, labels=None):
    data = {}
    i = 0
    for run_id in run_ids:
        for behavior in behaviors:
            behavior_path = os.path.join(results_path, run_id, behavior)
            name = (
                behavior
                if len(run_ids) == 1
                else (run_id if len(behaviors) == 1 else f"{run_id}/{behavior}")
            )
            if labels is not None:
                name = labels[i]
                i += 1
            df = get_scalar_dataframe_from_tb_files([behavior_path])
            data[name] = df
    return data


done_run_ids = [w_f_s, wo_f_s, w_f_ns, wo_f_ns, w_nf_s, wo_nf_s]

steps = {}
for run_id in done_run_ids:
    data = get_scalar_data(results_path, [run_id], behaviors)
    temp = {label: min(5000000, d["Timestep"].max()) for label, d in data.items()}
    temp["sum"] = sum(temp.values())
    steps[run_id] = temp

steps_df = pd.DataFrame(steps).transpose()
print(steps)
print(steps_df)
print(steps_df.to_latex())


def time_series_all_behaviors(results_path, run_id, behaviors):
    data = get_scalar_data(results_path, [run_id], behaviors)
    plot_multi_series(data, (12, 7), title=run_id, store=run_id)


def time_series_one_behavior(results_path, run_ids, behavior, title, labels):
    data = get_scalar_data(results_path, run_ids, [behavior], labels)
    plot_multi_series(data, (5, 3), title=title, store=title)


for run_id in done_run_ids:
    time_series_all_behaviors(results_path, run_id, behaviors)

f_s = (w_f_s, wo_f_s)
labels = ("CBF", "No CBF")
for behavior in behaviors:
    time_series_one_behavior(
        results_path, f_s, behavior, f"fixedbridge.safeplace/{behavior}", labels
    )

f_ns = (w_f_ns, wo_f_ns)
labels = ("CBF", "No CBF")
for behavior in behaviors:
    time_series_one_behavior(
        results_path, f_ns, behavior, f"fixedbridge.notsafeplace/{behavior}", labels
    )

nf_s = (w_nf_s, wo_nf_s)
labels = ("CBF", "No CBF")
for behavior in behaviors:
    time_series_one_behavior(
        results_path, nf_s, behavior, f"notfixedbridge.safeplace/{behavior}", labels
    )

w_f = (w_f_s, w_f_ns)
labels = ("safeplace", "notsafeplace")
for behavior in behaviors:
    time_series_one_behavior(
        results_path, w_f, behavior, f"wcbf.fixedbridge/{behavior}", labels
    )

w_s = (w_f_s, w_nf_s)
labels = ("fixedbridge", "notfixedbridge")
for behavior in behaviors:
    time_series_one_behavior(
        results_path, w_s, behavior, f"wcbf.safeplace/{behavior}", labels
    )
