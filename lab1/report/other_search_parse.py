def parse_file(name: str) -> None:
    with open(name, 'r') as f:
        lines = f.read().split('\n')
    
    iters, nodes = [], []
    for l in lines:
        if l.startswith('Iter:'):
            iters.append(int(l[l.find(':') + 1:]))
        if l.startswith('Max O + C:'):
            nodes.append(int(l[l.find(':') + 1:]))
    
    i = 0
    avg_iters = 0
    print('iters')
    for _iter in iters:
        if i > 9:
            i = 0
            print(avg_iters / 10)
        avg_iters += _iter
        i += 1
    print(avg_iters / 10)

    i = 0
    avg_nodes = 0
    print('nodes')
    for node in nodes:
        if i > 9:
            i = 0
            print(avg_nodes / 10)
        avg_nodes += node
        i += 1
    print(avg_nodes / 10)

def parse_bidir(name: str) -> None:
    with open(name, 'r') as f:
        lines = f.read().split('\n')
    idx = []
    for (i, l) in enumerate(lines):
        if l.startswith('COMMON'):
            idx.append(i)
    
    iters = []
    nodes = []

    for i in idx:
        iter_line = lines[i + 1]
        iters.append(int(iter_line[iter_line.find(':') + 1:]))
        node_line = lines[i + 2]
        nodes.append(int(node_line[node_line.find(':') + 1:]))

    i = 0
    avg_iters = 0
    print('iters')
    for _iter in iters:
        if i > 9:
            i = 0
            print(avg_iters / 10)
        avg_iters += _iter
        i += 1
    print(avg_iters / 10)

    i = 0
    avg_nodes = 0
    print('nodes')
    for node in nodes:
        if i > 9:
            i = 0
            print(avg_nodes / 10)
        avg_nodes += node
        i += 1
    print(avg_nodes / 10)

parse_file('Width.txt')