using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<type>
{

    public class NodeG<T>
    {
        private bool visit = false;
        private bool way = false;
        public T data;
        public bool way1 = false;
        public bool way2 = false;
        public NodeG<T> parent = null;
        public int distance = 0;
        public int acumulate = 1000;
        public bool aiControl = false;
        public bool playerControl = false;
        public List<NodeG<T>> adjacency = new List<NodeG<T>>();
        public NodeG() { }
        public NodeG(T dat)
        {
            data = dat;
        }
        public NodeG(T dat, int dis)
        {
            data = dat;
            distance = dis;
        }
        public bool Visited
        {
            get { return visit; }
            set { visit = value; }
        }
        public bool Way
        {
            get { return way; }
            set { way = value; }
        }

    }
    public NodeG<type> root = null;
    public NodeG<type> last = null;
    public NodeG<type> current = null;
    public List<NodeG<type>> visited = new List<NodeG<type>>();
    public List<NodeG<type>> moveSpot = new List<NodeG<type>>();
    public List<KeyValuePair<NodeG<type>, bool>> obstacle = new List<KeyValuePair<NodeG<type>, bool>>();

    public List<type> lBord = new List<type>();
    public List<type> rBord = new List<type>();
    public Graph() { }
    public Graph(type dat) { root = new NodeG<type>(dat); }
    public Graph(type dat, int d) { root = new NodeG<type>(dat, d); }

    public bool IsEmpty()
    {
        if (root == null) { return true; }
        return false;
    }
    public bool Heuristic(NodeG<type> node)
    {
        bool ret = false;
        bool find = false;
        if (node != null && node.distance > -1) { ret = true; }
        for (short it = 0; it < obstacle.Count; it++)
        {
            if (obstacle[it].Key == node) { find = true; }
        }
        if (!find)
        {
            obstacle.Add(new KeyValuePair<NodeG<type>, bool>(node, ret));
        }
        return ret;
    }
    public bool winHeuristic(NodeG<type> node, bool character)
    {
        if (node.playerControl && character) { return true; }
        else if (node.aiControl && !character) { return true; }
        return false;
    }
    public bool MemoryHeuristic(NodeG<type> node)
    {
        for (short it = 0; it < obstacle.Count; it++)
        {
            if (obstacle[it].Key == node)
            {
                return obstacle[it].Value;
            }
        }
        return Heuristic(node);
    }
    void AllVisited()
    {
        while (visited.Count != 0)
        {
            current = visited[0];
            current.Visited = false;
            visited.RemoveAt(0);
        }
        current = null;
    }
    void AllVisitedR(NodeG<type> r)
    {
        if (r != null)
        {
            if (r.Visited)
            {
                r.Visited = false;
                for (int it = 0; it < r.adjacency.Count; it++)
                {
                    AllVisitedR(r.adjacency[it]);
                }
            }
        }
    }
    public NodeG<type> Intersects(List<NodeG<type>> first, List<NodeG<type>> second, List<NodeG<type>> parentsF, List<NodeG<type>> parentsS)
    {
        NodeG<type> ret = null;
        bool intersect = false;
        for (int it = 0; it < first.Count; it++)
        //foreach(NodeG<type> tmp1 in first)
        {
            if (intersect) { break; }
            for (int it2 = 0; it2 < second.Count; it2++)
            //foreach(NodeG<type> tmp2 in second)
            {
                //if (tmp1.Equals(tmp2))
                if(first[it].Equals(second[it2]))
                {
                    ret = first[it];
                    ret.way1 = true;
                    ret.way2 = true;
                    intersect = true;
                    break;
                }
            }
        }
        if (ret != null)
        {
            bool path = false;
            for (int it = 0; it < parentsF.Count; it++)
            {
                path = false;
                for (int it2 = 0; it2 < parentsF[it].adjacency.Count; it++)
                {
                    if (parentsF[it].adjacency[it2].way1 && !parentsF[it].adjacency[it2].Way)
                    {
                        path = true;
                    }
                }
                if (!path && !parentsF[it].Equals(parentsF[0]))
                {
                    parentsF[it].Way = false;
                }
                else
                {
                    parentsF[it].Way = true;
                }
            }
            for (int it = 0; it < parentsS.Count; it++)
            {
                path = false;
                for (int it2 = 0; it2 < parentsS[it].adjacency.Count; it2++)
                {
                    if (parentsS[it].adjacency[it2].way2 && !parentsS[it].adjacency[it2].Way)
                    {
                        path = true;
                    }
                }
                if (!path && !parentsS[it].Equals(parentsS[it]))
                {
                    parentsS[it].Way = false;
                }
                else
                {
                    parentsS[it].Way = true;
                }
            }
        }
        return ret;
    }
    public NodeG<type> BFS(type dat)
    {
        List<NodeG<type>> queue = new List<NodeG<type>>();
        List<NodeG<type>> tmp = new List<NodeG<type>>();
        NodeG<type> ret = null;
        queue.Add(root);
        if (queue[0] != null)
        {
            while (queue.Count != 0)
            {
                current = queue[0];
                if (current.data.Equals(dat)) //Aqui talvez esta mal
                {
                    ret = current;
                    
                    break;
                }
                else
                {
                    for (int it = 0; it < current.adjacency.Count; it++)
                    {
                        if (!current.adjacency[it].Visited)
                        {
                            queue.Add(current.adjacency[it]);
                        }
                    }
                }
                current.Visited = true;
                visited.Add(current);
                tmp.Add(current);
                queue.RemoveAt(0);
            }
        }
        moveSpot = tmp;
        current = null;
        AllVisited();
        if(ret!= null)
        {
            moveSpot.Add(ret);
        }
        return ret;
    }
    public NodeG<type> DFS(type dat)
    {
        List<NodeG<type>> stack = new List<NodeG<type>>();
        List<NodeG<type>> tmp = new List<NodeG<type>>();
        NodeG<type> ret = null;
        stack.Add(root);
        current = stack[0];
        while (current != null)
        {
            current = stack[0];
            if (current.data.Equals(dat))
            {
                ret = current;
                break;
            }
            current.Visited = true;
            visited.Add(current);
            tmp.Add(current);
            stack.RemoveAt(0);
            for (int it = current.adjacency.Count; it > 0; it--)
            {
                if (!current.adjacency[it-1].Visited)
                {
                    stack.Insert(0,current.adjacency[it-1]);
                }
            }
            if (stack.Count == 0)
            {
                current = null;
            }
        }
        moveSpot = tmp;
        current = null;
        AllVisited();
        if (ret != null)
        {
            moveSpot.Add(ret);
        }
        return ret;
    }
        List<NodeG<type>> tmpDLS = new List<NodeG<type>>();
    public NodeG<type> DLS(NodeG<type> start, int limit, type dat)
    {

        NodeG<type> ret = null;
        if (start != null && start.data.Equals(dat))
        {
            ret = start;
        }
        else if (start != null && limit > 0)
        {
            for (int it = 0; it < start.adjacency.Count; it++)
            {
                if (!start.adjacency[it].Visited)
                {
                    start.adjacency[it].Visited = true;
                    visited.Add(start.adjacency[it]);
                    if (!start.adjacency[it].Way)
                    {
                        tmpDLS.Add(start.adjacency[it]);
                    }
                    start.adjacency[it].Way = true;
                    ret = DLS(start.adjacency[it], limit - 1, dat);
                    if (ret != null && ret.data.Equals(dat))
                    {
                        break;
                    }
                }
            }
        }
        moveSpot = tmpDLS;
        AllVisited();
        if (ret != null)
        {
            moveSpot.Add(ret);
        }
        return ret;
    }
    public NodeG<type> IDDFS(NodeG<type> start, int depth, type dat)
    {

        NodeG<type> ret = null;
        if (start != null)
        {
            int i = 0;
            while (i < depth + 1)
            {
                i++;
                NodeG<type> end = DLS(start, i, dat);
                if (end != null )
                {
                    //moveSpot.Add(end);
                    if (end.data.Equals(dat))
                    {
                        ret = end;
                        break;
                    }
                }
            }
        }
        /*if (ret != null)
        {
            moveSpot.Add(ret);
        }*/
        return ret;
    }
    public bool Bidirectional(NodeG<type> start, NodeG<type> end, bool character)
    {
        bool cross = false;
        if (start != null && end != null)
        {
            List<NodeG<type>> visitF = new List<NodeG<type>>(),
                visitS = new List<NodeG<type>>(),
                parentsF = new List<NodeG<type>>(),
                parentsS = new List<NodeG<type>>(),
                first = new List<NodeG<type>>(),
                second = new List<NodeG<type>>();
            List<NodeG<type>> tmp = new List<NodeG<type>>();
            first.Add(start);
            start.Visited = true;
            second.Add(end);
            end.Visited = true;

            while (first.Count != 0 && second.Count != 0)
            {
                NodeG<type> inter = Intersects(visitF, visitS, parentsF, parentsS);
                if (inter != null)
                {
                    cross = true;
                    inter.Way = true;
                    parentsF.Add(inter);
                    break;
                }
                current = first[0];
                first.RemoveAt(0);
                for (int it = 0; it < current.adjacency.Count; it++)
                {
                    if (!current.adjacency[it].Visited && !current.adjacency[it].way1 && winHeuristic(current.adjacency[it], character))
                    {
                        current.adjacency[it].Visited = true;
                        visitF.Add(current.adjacency[it]);
                        first.Add(current.adjacency[it]);
                    }
                }
                parentsF.Add(current);
                current.way1 = true;
                for (int it = 0; it < visitF.Count; it++)
                {
                    visitF[it].Visited = false;
                }
                current = second[0];
                second.RemoveAt(0);
                for (int it = 0; it < current.adjacency.Count; it++)
                {
                    if (!current.adjacency[it].Visited && !current.adjacency[it].way2 && winHeuristic(current.adjacency[it], character))
                    {
                        current.adjacency[it].Visited = true;
                        visitS.Add(current.adjacency[it]);
                        second.Add(current.adjacency[it]);
                    }
                }
                parentsS.Add(current);
                current.way2 = true;
                for (int it = 0; it < visitS.Count; it++)
                {
                    visitS[it].Visited = false;
                }
            }

            //Debug.Log("\nEl camino bidireccional fue:\n");
            while (parentsF.Count != 0)
            {
                NodeG<type> ruteF = parentsF[0];
                if (ruteF.Way)
                {
                    //Debug.Log(ruteF.data + "->");
                    tmp.Add(ruteF);
                }
                ruteF.Way = false;
                ruteF.way1 = false;
                ruteF.way2 = false;
                parentsF.RemoveAt(0);
            }
            while (parentsS.Count != 0)
            {
                NodeG<type> ruteS = parentsS[parentsS.Count - 1];
                if (ruteS.Way)
                {
                    //Debug.Log(ruteS.data + "->");
                    tmp.Add(ruteS);
                }
                ruteS.Way = false;
                ruteS.way1 = false;
                ruteS.way2 = false;
                parentsS.RemoveAt(parentsS.Count - 1);
            }
            start.Visited = false;
            end.Visited = false;
            current = null;
            //moveSpot = tmp;
        }
        return cross;
    }
    public void Dijkstra(NodeG<type> start, NodeG<type> end)
    {
        if (start != null && end != null)
        {
            int s = start.distance;
            int min = 0;
            start.distance = 0;
            start.acumulate = 0;
            bool foundPath = false;
            List<NodeG<type>> path = new List<NodeG<type>>();
            List<NodeG<type>> parents = new List<NodeG<type>>();
            path.Add(start);
            parents.Add(start);
            start.Way = true;
            while (path.Count != 0)
            {
                if (foundPath) { break; }
                foundPath = false;
                current = path[0];
                path.RemoveAt(0);
                min = 1000;
                NodeG<type> minCost = current;
                foreach (NodeG<type> tmpNode in current.adjacency)
                {
                    int dist = tmpNode.distance + current.acumulate;
                    if (tmpNode == end)
                    {
                        end.Way = true;
                        current.Way = true;
                        end.acumulate = dist;
                        parents.Add(current);
                        parents.Add(end);
                        foundPath = true;
                        break;
                    }
                    else if (dist < tmpNode.acumulate && Heuristic(tmpNode) && !tmpNode.Visited)
                    {
                        tmpNode.acumulate = dist;
                    }
                    if (tmpNode.acumulate < min && Heuristic(tmpNode) && !tmpNode.Visited)
                    {
                        min = tmpNode.acumulate;
                        minCost = tmpNode;
                    }
                }
                if (!minCost.Visited)
                {
                    minCost.Visited = true;
                    path.Add(minCost);
                    visited.Add(minCost);
                }
                if (!current.Way)
                {
                    current.Way = true;
                    parents.Add(current);
                }
            }
            List<NodeG<type>> tmp = new List<NodeG<type>>();
            Debug.Log("\nEl camino Djikstra fue:\n");
            for (int it = 0; it < parents.Count; it++)
            {
                if (parents[it].Way)
                {
                    tmp.Add(parents[it]);
                    Debug.Log(parents[it].data + "->");
                }
            }
            while (visited.Count != 0)
            {
                NodeG<type> v = visited[visited.Count - 1];
                v.Visited = false;
                v.Way = false;
                v.acumulate = 1000;
                visited.RemoveAt(visited.Count - 1);
            }
            moveSpot = tmp;
        }
    }
    public void AStar(NodeG<type> start, NodeG<type> end)
    {
        if (start != null && end != null)
        {
            start.acumulate = 0;
            int min = 0;
            bool foundPath = false;
            List<NodeG<type>> path = new List<NodeG<type>>();
            List<NodeG<type>> parents = new List<NodeG<type>>();
            path.Add(start);
            start.Way = true;
            while (path.Count != 0) 
            {
                if (foundPath) { break; }
                foundPath = false;
                current = path[0];
                path.RemoveAt(0);
                min = 6;
                NodeG<type> minCost = current;
                foreach (NodeG<type> tmp in current.adjacency)
                {
                    int dist = 1 + current.acumulate;
                    if (tmp == end)
                    {
                        end.Way = true;
                        current.Way = true;
                        end.acumulate = dist;
                        parents.Add(current);
                        parents.Add(end);
                        foundPath = true;
                        break;
                    }
                    else if (dist <= tmp.acumulate && Heuristic(tmp) && !tmp.Visited)
                    {
                        tmp.acumulate = dist;
                    }
                    if (tmp.acumulate <= min && Heuristic(tmp) && !tmp.Visited)
                    {
                        min = tmp.acumulate;
                        minCost = tmp;
                    }
                }
                if (!minCost.Visited)
                {
                    minCost.Visited = true;
                    path.Add(minCost);
                    visited.Add(minCost);
                }
                if (!current.Way)
                {
                    current.Way = true;
                    parents.Add(current);
                }
            }
            Debug.Log("\nEl camino A* fue:\n");
            foreach (NodeG<type> tmp in parents)
            {
                if (tmp.Way)
                {
                    Debug.Log(tmp.data);
                }
            }
            moveSpot = parents;
            while (visited.Count != 0)
            {
                NodeG<type> v = visited[0];
                v.Visited = false;
                v.Visited = false;
                v.acumulate = 1000;
                visited.RemoveAt(0);
            }
        }
    }
    public void HillClimbing(NodeG<type> start)
    {
        if (start != null)
        {
            List<NodeG<type>> destination = new List<NodeG<type>>();
            destination.Add(start);
            visited.Add(start);
            NodeG<type> better = start;
            start.Visited = true;
            start.Way = true;
            while (destination.Count != 0)
            {
                current = destination[0];
                destination.RemoveAt(0);
                for (int it = 0; it < current.adjacency.Count; it++)
                {
                    if (current.adjacency[it].distance > current.distance && Heuristic(current.adjacency[it]))
                    {
                        better = current.adjacency[it];
                    }
                }
                if (!better.Visited)
                {
                    visited.Add(better);
                    destination.Add(better);
                    better.Visited = true;
                    better.Way = true;
                }
            }
            Debug.Log("\nEl camino Hill Climbing fue:\n");
            while (visited.Count != 0)
            {
                NodeG<type> tmp = visited[0];
                Debug.Log(tmp.data + "->");
                tmp.Visited = true;
                tmp.Way = false;
                visited.RemoveAt(0);
            }
        }
    }
    public void InsertBFS(type dataNode, type dat, int dis)
    {
        NodeG<type> curr = BFS(dataNode);
        if (IsEmpty())
        {
            root = new NodeG<type>(dat);
            root.distance = dis;
        }
        else if (curr != null)
        {
            NodeG<type> son = BFS(dat);
            List<NodeG<type>> currVec = curr.adjacency;
            if (curr != null && son != null)
            {
                List<NodeG<type>> sonVec = son.adjacency;
                son.parent = curr;
                bool find = false;
                for (int it = 0; it < currVec.Count; it++)
                {
                    if (currVec[it] == son)
                    {
                        find = true;
                    }
                }
                if (!find)
                {
                    curr.adjacency.Add(son);
                    find = false;
                }
                if (curr.parent != null)
                {
                    curr.parent = son;
                }
            }
            else
            {
                NodeG<type> tmp = new NodeG<type>(dat);
                tmp.parent = curr;
                curr.adjacency.Add(tmp);
                tmp.adjacency.Add(curr);
                tmp.distance = dis;
                last = tmp;
            }
        }
    }
    public void print()
    {
        List<NodeG<type>> visite = new List<NodeG<type>>();
        visite.Add(root);
        while (visite.Count != 0)
        {
            current = visite[0];
            Debug.Log("\n(" + current.data + ')');
            //NodeG<type> tmp = current.adjacency[0];
            for (int it = 0; it < current.adjacency.Count; it++)
            {
                Debug.Log("->" + current.adjacency[it].data);
                if (!current.adjacency[it].Visited)
                {
                    visite.Add(current.adjacency[it]);
                    current.adjacency[it].Visited = true;
                }
            }
            current.Visited = true;
            visite.RemoveAt(0);
        }
        AllVisitedR(root);
    }
    public List<NodeG<type>> getMoveSpots(type dat)
    {

        return moveSpot;
    }
}
