### Python Environment

(Specific to my computer, should not be in ReadMe)

```
conda deactivate
source ~/python-envs/ma/bin/activate
cd ~/dev/unity/RLTest
```

### Dependencies

- Netwonsoft.Json (https://www.nuget.org/packages/Newtonsoft.Json)

Install nuget packages: https://www.youtube.com/watch?v=rRILew38aWY

Possibly an alternative way to install Netwonsoft.Json: https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM

### Training


```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --env=builds/build_env5_wcbf
```

#### Resume
```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --env=builds/build_env5_wcbf --resume
```

### Evaluation

```
mlagents-learn configs/conf_wcbf.yaml --run-id=env5.wcbf.2 --resume --inference
```

Currently, evaluation does only work in unity an not in built environments.

### Tensorboard

```
tensorboard --logdir=results
```
