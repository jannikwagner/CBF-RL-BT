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

### Training


```
mlagents-learn configs/conf2.yaml --env=builds/build10 --run-id=env5.26 --initialize-from=env5.25
```
#### Resume
```
mlagents-learn configs/conf2.yaml --env=builds/build10 --run-id=env5.26 --resume
```

### Evaluation

```
mlagents-learn configs/conf2.yaml --env=builds/build16 --run-id=env5.31 --resume --inferenc
```

### Tensorboard

```
tensorboard --logdir=results
```
