from dataclasses import dataclass
from typing import Sequence

@dataclass
class Condition:
    name: str

@dataclass
class Action:
    name: str

C_B2 = Condition("B2 pressed")
C_PB = Condition("Past bridge")
C_OB = Condition("On bridge")
C_B1 = Condition("B1 pressed")
C_UP = Condition("Up")
C_T2 = Condition("Controlling T2")
C_T1 = Condition("Controlling T1")

conditions = [C_B2, C_PB, C_OB, C_B1, C_UP, C_T2, C_T1]

print(C_B2)

A_MB2 = Action("Move to B2")
A_MB1 = Action("Move to B1")
A_MT2 = Action("Move to T2")
A_MT1 = Action("Move to T1")
A_MUP = Action("Move up")
A_MOB = Action("Move over bridge")
A_MTB = Action("Move to bridge")

actions = [A_MB2, A_MB1,A_MT2, A_MT1, A_MUP, A_MOB, A_MTB]

@dataclass
class PPA_Triple:
    postcondition: Condition
    preconditions: Sequence[Condition]
    action: Action

ppas = [
    PPA_Triple(C_B2, (C_PB, ), A_MB2),
    PPA_Triple(C_PB, (C_OB, ), A_MOB),
    PPA_Triple(C_OB, (C_B1, C_T2, C_UP, ), A_MTB),
    PPA_Triple(C_B1, (C_T1, C_UP, ), A_MB1),
    PPA_Triple(C_T2, (C_B1, ), A_MT2),
    PPA_Triple(C_T1, ( ), A_MT1),
    PPA_Triple(C_UP, ( ), A_MUP),
]

def build_tree(goal: Condition, ppas: Sequence[PPA_Triple]):
    ppa = [ppa for ppa in ppas if ppa.postcondition == goal][0]
    precondition_trees = [build_tree(precondition, ppas) for precondition in ppa.preconditions]
    return ("F", ppa.postcondition, ("S", *precondition_trees, ppa.action))


print(build_tree(C_B2, ppas))