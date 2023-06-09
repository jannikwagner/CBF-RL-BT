### Python Environment

(Specific to my computer, should not be in ReadMe)

```
conda deactivate
source ~/python-envs/ma/bin/activate
cd ~/dev/unity/RLTest
```

### Training


```
mlagents-learn configs/conf2.yaml --env=builds/build10 --run-id=env5.26 --initialize-from=env5.25
```
#### Resume
```
mlagents-learn configs/conf2.yaml --env=builds/build10 --run-id=env5.26 --resume
```

### Tensorboard

```
tensorboard --logdir=results
```
