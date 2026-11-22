# CBF-RL-BT

Code for my master's thesis at KTH Royal Institute of Technology (2023):
[*Improving Behavior Trees that Use Reinforcement Learning with Control Barrier Functions*](https://www.diva-portal.org/smash/record.jsf?pid=diva2%3A1837748).

The thesis combines behavior trees whose actions are learned with reinforcement learning and control barrier functions (CBFs) that keep each action from violating the conditions other parts of the tree rely on. With CBFs, the agents need significantly less training for comparable or slightly better results, and they violate their constraints considerably less often.

The work is the basis of the paper:

> M. Kartašev, J. Wagner, P. Ögren. [*Using Control Barrier Functions for Constrained Reinforcement Learning in Backward Chained Behavior Trees*](https://doi.org/10.1109/CDC57313.2025.11312735). IEEE 64th Conference on Decision and Control (CDC), 2025, pp. 3127–3134.

## Contents

| Path | Content |
| --- | --- |
| `Assets/` | Unity project with the environments, agents, behavior trees and CBFs (C#) |
| `configs/` | ML-Agents training configurations, with and without CBFs |
| `evaluation/` | Python scripts that evaluate the runs and produce the tables |

## Setup

- Unity with the [ML-Agents](https://github.com/Unity-Technologies/ml-agents) package
- Python packages: `pip install -r requirements.txt`
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) for Unity, for example [via UPM](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM)

## Training

```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --env=builds/build_env5_wcbf
```

Add `--resume` to continue a run:

```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --env=builds/build_env5_wcbf --resume
```

## Evaluation

```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --resume --inference
```

Evaluation only works inside the Unity editor, not in built environments.

## TensorBoard

```
tensorboard --logdir=results
```

## Citation

See `CITATION.cff` for the software, or cite the paper above.
