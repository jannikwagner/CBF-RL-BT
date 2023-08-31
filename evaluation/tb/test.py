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

results_path = "results"
run_id = "env5.wcbf.fixedbridge.safeplace"
run_ids = ["env5.wcbf.fixedbridge.safeplace", "env5.wocbf.fixedbridge.safeplace"]
run_ids = ["env5.wcbf.notfixedbridge.safeplace"]
run_path = os.path.join(results_path, run_id)
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


def get_scalar_data(results_path, run_ids, behaviors):
    data = {}
    for run_id in run_ids:
        for behavior in behaviors:
            behavior_path = os.path.join(results_path, run_id, behavior)
            name = behavior if len(run_ids) == 1 else f"{run_id}/{behavior}"
            df = get_scalar_dataframe_from_tb_files([behavior_path])
            data[name] = df
    return data


data = get_scalar_data(results_path, run_ids, behaviors)
print(data.keys())
behavior_path = "results/env5.wcbf.fixedbridge.safeplace/MoveToTrigger2/"
event_path = "results/env5.wcbf.fixedbridge.safeplace/MoveToTrigger2/events.out.tfevents.1692266249.DESKTOP-EIN0CD2.33384.1"
event_path2 = "results/env5.wcbf.fixedbridge.safeplace/MoveToTrigger2/events.out.tfevents.1693384384.MBP-von-Ambient.3110.0"

plot_multi_series(data, (12, 7), title="")

hist_df = get_hist_dataframe_from_tb_files([event_path, event_path2])
num_steps = 10
steps = np.linspace(0, len(hist_df) - 1, num_steps, dtype=int)
print(steps)
print(hist_df)
plot_histogram(hist_df, steps, "")
