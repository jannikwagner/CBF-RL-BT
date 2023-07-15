from dataclasses import dataclass
from typing import Sequence, Tuple


class Node:
    pass


class ExecutionNode(Node):
    pass


@dataclass
class Condition(ExecutionNode):
    name: str


@dataclass
class Action(ExecutionNode):
    name: str


@dataclass
class CompositeNode(Node):
    children: Sequence[Node]


class SequenceNode(CompositeNode):
    symbol = "S"


class Fallback(CompositeNode):
    symbol = "F"


C_B2 = Condition("B2 pressed")
C_PB = Condition("Past bridge")
C_OB = Condition("On bridge")
C_B1 = Condition("B1 pressed")
C_UP = Condition("Up")
C_T2 = Condition("Controlling T2")
C_T1 = Condition("Controlling T1")

conditions = [C_B2, C_PB, C_OB, C_B1, C_UP, C_T2, C_T1]

A_MB2 = Action("Move to B2")
A_MB1 = Action("Move to B1")
A_MT2 = Action("Move to T2")
A_MT1 = Action("Move to T1")
A_MUP = Action("Move up")
A_MOB = Action("Move over bridge")
A_MTB = Action("Move to bridge")

actions = [A_MB2, A_MB1, A_MT2, A_MT1, A_MUP, A_MOB, A_MTB]


@dataclass
class PPA:
    postcondition: Condition
    preconditions: Sequence[Condition]
    action: Action


ppas1 = [
    PPA(C_B2, (C_PB,), A_MB2),
    PPA(C_PB, (C_OB,), A_MOB),
    PPA(
        C_OB,
        (
            C_B1,
            C_T2,
            C_UP,
        ),
        A_MTB,
    ),
    PPA(
        C_B1,
        (
            C_T1,
            C_UP,
        ),
        A_MB1,
    ),
    PPA(C_T2, (C_B1,), A_MT2),
    PPA(C_T1, (), A_MT1),
    PPA(C_UP, (), A_MUP),
]

ppas2 = [
    PPA(C_B2, (C_PB,), A_MB2),
    PPA(C_PB, (C_OB,), A_MOB),
    PPA(
        C_OB,
        (
            C_B1,
            C_T2,
            C_UP,
        ),
        A_MTB,
    ),
    PPA(
        C_B1,
        (
            C_T1,
            C_UP,
        ),
        A_MB1,
    ),
    PPA(C_T2, (), A_MT2),
    PPA(C_T1, (), A_MT1),
    PPA(C_UP, (), A_MUP),
]

ppas3 = [
    PPA(C_B2, (C_PB,), A_MB2),
    PPA(C_PB, (C_OB,), A_MOB),
    PPA(
        C_OB,
        (
            C_T2,
            C_UP,
        ),
        A_MTB,
    ),
    PPA(
        C_B1,
        (
            C_T1,
            C_UP,
        ),
        A_MB1,
    ),
    PPA(C_T2, (C_B1,), A_MT2),
    PPA(C_T1, (), A_MT1),
    PPA(C_UP, (), A_MUP),
]

# current
ppas4 = [
    PPA(
        C_B2,
        (
            C_B1,
            C_T2,
            C_UP,
            C_PB,
        ),
        A_MB2,
    ),
    PPA(
        C_B1,
        (
            C_T1,
            C_UP,
        ),
        A_MB1,
    ),
    PPA(C_T2, (), A_MT2),
    PPA(C_UP, (), A_MUP),
    PPA(C_T1, (), A_MT1),
    PPA(C_OB, (), A_MTB),
    PPA(C_PB, (C_OB,), A_MOB),
]


def build_tree(goal: Condition, ppas: Sequence[PPA]):
    ppa = [ppa for ppa in ppas if ppa.postcondition == goal][0]
    precondition_trees = [
        build_tree(precondition, ppas) for precondition in ppa.preconditions
    ]
    return Fallback(
        [ppa.postcondition, SequenceNode([*precondition_trees, ppa.action])]
    )


def print_tree(tree: Node, depth=0):
    prefix = "| " * depth
    if isinstance(tree, CompositeNode):
        symbol = tree.symbol
        print(prefix + symbol)
        for child in tree.children:
            print_tree(child, depth + 1)
    elif isinstance(tree, Action):
        print(f"{prefix}[{tree.name}]")
    elif isinstance(tree, Condition):
        print(f"{prefix}({tree.name})")
    else:
        print(type(tree))
        print(tree)
        raise NotImplementedError


def accs_bottom_up(
    path: Sequence[Node],
) -> Tuple[Sequence[Condition], Sequence[Condition]]:
    preconditions = []
    accs = []
    assert isinstance(path[-1], Action)
    assert isinstance(path[0], Fallback)
    for i, node in enumerate(path):
        if isinstance(node, Fallback):
            assert node.children[1] == path[i + 1]
            assert isinstance(node.children[0], Condition)
        elif isinstance(node, SequenceNode):
            accs.extend(preconditions)
            preconditions = []
            for child in node.children:
                if child == path[i + 1]:
                    break
                assert isinstance(child, (Condition, Fallback))
                if isinstance(child, Condition):
                    preconditions.append(child)
                elif isinstance(child, Fallback):
                    assert isinstance(child.children[0], Condition)
                    preconditions.append(child.children[0])
        else:
            assert isinstance(node, Action)
            assert node == path[-1]
    return accs, preconditions


def accs_bottom_up_loop(
    tree: Node,
) -> Sequence[Tuple[Action, Sequence[Condition], Sequence[Condition]]]:
    path = [tree]
    pointers = [0]
    accs = []
    while path:
        node = path[-1]
        if isinstance(node, Action):
            node_accs, node_preconditions = accs_bottom_up(path)
            accs.append((node, node_accs, node_preconditions))
        if isinstance(node, ExecutionNode):
            path.pop()
            pointers.pop()
        elif isinstance(node, CompositeNode):
            if pointers[-1] < len(node.children):
                path.append(node.children[pointers[-1]])
                pointers[-1] += 1
                pointers.append(0)
            else:
                path.pop()
                pointers.pop()
        else:
            raise NotImplementedError
    return accs


ppas = ppas4
goal = C_B2

bt = build_tree(goal, ppas)
print_tree(bt)
print()

action_acc_precondition = accs_bottom_up_loop(bt)
# print(action_acc_precondition)
for action, accs, preconditions in action_acc_precondition:
    print(action)
    print("accs:", accs)
    print("preconditions:", preconditions)
    print()
    ppa = [ppa for ppa in ppas if ppa.action == action][0]
    assert list(ppa.preconditions) == list(preconditions)
