import csv
import os
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
import seaborn as sns

from tbparse import SummaryReader


def get_scalar_dataframe_from_tb_files(log_dirs):
    reader = SummaryReader(log_dirs[0], pivot=True)

    def rename_axis(df):
        return df.rename(
            columns={
                "Environment/Cumulative Reward": "Mean Reward",
                "step": "Timestep",
                "Losses/Policy Loss": "Loss",
            }
        )

    df = rename_axis(reader.scalars)

    for log_dir in log_dirs[1:]:
        reader = SummaryReader(log_dir, pivot=True)
        new_df = rename_axis(reader.scalars)
        df = pd.concat([df, new_df])
    return df


def get_hist_dataframe_from_tb_files(log_dirs):
    reader = SummaryReader(log_dirs[0], pivot=True)

    def rename_axis(df):
        return df.rename(
            columns={
                "step": "Timestep",
            }
        )

    df = rename_axis(reader.histograms)
    for log_dir in log_dirs[1:]:
        reader = SummaryReader(log_dir, pivot=True)
        df = pd.concat([df, rename_axis(reader.histograms)])
    return df


def get_reward_series(filename):
    timesteps = list()
    values = list()
    with open(filename, newline="") as csvfile:
        spamreader = csv.reader(csvfile, delimiter=",", quotechar="|")
        for row in spamreader:
            if row[0] == "Wall time":
                continue
            timesteps.append(float(row[1]))
            values.append(float(row[2]))
    df = pd.DataFrame()
    df["Timestep"] = timesteps
    df["Mean Reward"] = values
    return df


def plot_reward_series(data, spacing, figsize, yrange):
    # Make a data frame

    plt.style.use("seaborn-darkgrid")

    # create a color palette
    palette = plt.get_cmap("Set1")

    # multiple line plot
    num = 0
    fig = plt.figure(figsize=figsize)
    for name, values in data.items():
        num += 1

        # Find the right spot on the plot
        plt.subplot(spacing[0], spacing[1], num)

        for _, shadow in data.items():
            sns.lineplot(
                x="Timestep", y="Mean Reward", data=shadow, color="grey", alpha=0.2
            )
        # Plot the lineplot
        sns.lineplot(x="Timestep", y="Mean Reward", data=values)
        plt.ylim((yrange))

        # if num < 7:
        #     plt.xticks(ticks=[])
        #     plt.xlabel(None)
        plt.title(name, loc="left", fontsize=8, fontweight=0)

    plt.tight_layout()
    plt.show()


def plot_multi_series(
    data,
    figsize,
    title="Mean reward for last experience buffer",
    x_axis="Timestep",
    y_axis="Mean Reward",
    store=None,
):
    # Make a data frame

    sns.set_context("paper")
    sns.set_style("darkgrid")
    sns.set(font_scale=1.41)
    # create a color palette
    palette = plt.get_cmap("Set1")
    fig = plt.figure(figsize=figsize)
    # f, ax = plt.subplots(1, 1)

    # handles = list()
    for name, values in data.items():
        mask = [np.isscalar(x) for x in values[y_axis]]
        if not all(mask):
            idx = np.where(np.logical_not(mask))
            print(name, "has non scalar entries:", values[y_axis].iloc[idx])
        values = values[mask]
        subplot = sns.lineplot(
            x=x_axis,
            y=y_axis,
            data=values.drop_duplicates().reset_index(drop=True),
            label=name,
        )
        # handles.extend(subplot)

    # plt.legend(handles=handles, fontsize=17)
    plt.title(title, loc="left", fontsize=16, fontweight=0)
    plt.tight_layout()
    if store:
        path = f"evaluation/tb/plots/{store}.pdf"
        folder = os.path.dirname(path)
        os.makedirs(folder, exist_ok=True)
        plt.savefig(path)
        plt.cla()
        plt.close()
    else:
        plt.show()


def plot_histogram(
    df,
    steps_to_plot=None,
    title="Distribution of cumulative episode reward per training session",
):
    # Set background
    sns.set_theme(style="white", rc={"axes.facecolor": (0, 0, 0, 0)})

    # Choose color palettes for the distributions
    pal = sns.color_palette("Blues", 25)[15:-1]
    # Initialize the FacetGrid object (stacking multiple plots)
    df = df.take(steps_to_plot)
    g = sns.FacetGrid(
        df, row="Timestep", hue="Timestep", aspect=15, height=0.5, palette=pal
    )

    def plot_subplots(x, color, label, data):
        ax = plt.gca()
        ax.text(
            -0.05,
            0.08,
            label,
            fontweight="bold",
            color=color,
            ha="left",
            va="center",
            transform=ax.transAxes,
        )
        counts = data["Environment/Cumulative Reward_hist/counts"].iloc[0]
        limits = data["Environment/Cumulative Reward_hist/limits"].iloc[0]
        x, y = SummaryReader.histogram_to_bins(
            counts, limits, lower_bound=-3, upper_bound=3, n_bins=50
        )
        # Draw the densities in a few steps
        y = np.log2(y)
        sns.lineplot(x=x, y=y, clip_on=False, color="w", lw=0.3)
        ax.fill_between(x, y, color=color)
        ax.set_ylim([0, 25])

    # Plot each subplots with df[df['step']==i]
    g.map_dataframe(plot_subplots, None)

    # Add a bottom line for each subplot
    # passing color=None to refline() uses the hue mapping
    g.refline(y=0, linewidth=0.4, linestyle="-", color=None, clip_on=False)
    # Set the subplots to overlap (i.e., height of each distribution)
    g.figure.subplots_adjust(hspace=-0.8)
    # Remove axes details that don't play well with overlap
    g.set_titles("")
    g.axes.flatten()[1].set_title(title, fontsize=16, fontweight=0)
    g.set(yticks=[], xlabel="", ylabel="")
    g.despine(bottom=True, left=True)
    plt.show()
