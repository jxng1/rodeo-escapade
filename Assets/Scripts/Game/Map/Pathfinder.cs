using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    public class Node<T>
    {

        public int id { get; private set; }

        public List<Node<T>> neighbours { get; }

        public T data { get; private set; }

        public Node(int id, T data)
        {
            this.id = id;
            this.neighbours = new List<Node<T>>();

            if (data != null)
            {
                this.data = data;
            }
            else
            {
                Debug.LogError("Data is null!");
                return;
            }
        }

        public void addNeighbour(Node<T> neighbour)
        {
            if (!neighbours.Contains(neighbour))
            {
                neighbours.Add(neighbour);
            }
        }
        public void removeNeighbour(Node<T> neighbour)
        {
            if (neighbours.Contains(neighbour))
            {
                neighbours.Remove(neighbour);
            }
            else
            {
                Debug.Log("This node doesn't have such neighbour!");
            }
        }
    }

    public class Graph<T>
    {
        public List<Node<T>> nodes { get; private set; }
        public int graphSize { get; }

        private float[,] adjMatrix;

        public Graph(int graphSize)
        {
            nodes = new List<Node<T>>();
            this.graphSize = graphSize;

            createGraph();
        }

        private void createGraph()
        {
            adjMatrix = new float[graphSize + 1, graphSize + 1];
        }

        public Node<T> addNode(int id, T data)
        {
            Node<T> newNode = new Node<T>(id, data);

            nodes.Add(newNode);

            return newNode;
        }

        public void addUndirectedEdge(int nodeA, int nodeB, float cost)
        {
            if (nodeA > 0 && nodeB > 0 && nodeA <= graphSize && nodeB <= graphSize)
            {
                adjMatrix[nodeA, nodeB] = cost;
                adjMatrix[nodeB, nodeA] = cost;

                nodes.Find(node => node.id == nodeA).addNeighbour(nodes.Find(node => node.id == nodeB));
                nodes.Find(node => node.id == nodeB).addNeighbour(nodes.Find(node => node.id == nodeA));
            }
        }

        public void removeUndirectedEdge(int nodeA, int nodeB)
        {
            if (nodeA > 0 && nodeB > 0 && nodeA <= graphSize && nodeB <= graphSize)
            {
                adjMatrix[nodeA, nodeB] = 0;
                adjMatrix[nodeB, nodeA] = 0;

                nodes.Find(node => node.id == nodeA).removeNeighbour(nodes.Find(node => node.id == nodeB));
                nodes.Find(node => node.id == nodeB).removeNeighbour(nodes.Find(node => node.id == nodeA));
            }
        }

        public void printDebugGraph()
        {
            for (int i = 0; i < graphSize + 1; i++)
            {
                for (int j = 0; j < graphSize + 1; j++)
                {
                    Debug.Log("i: " + i + " j: " + j + " " + adjMatrix[i, j]);
                }
            }
        }

        public bool isConnected(int nodeA, int nodeB)
        {
            return getPathCost(nodeA, nodeB) + getPathCost(nodeB, nodeA) > 0;
        }

        public float getPathCost(int nodeA, int nodeB)
        {
            return adjMatrix[nodeA, nodeB];
        }

        public List<Node<T>> primSpanningTree()
        {
            if (graphSize < 1)
            {
                return null;
            }

            List<Node<T>> tree = new List<Node<T>>();
            Node<T> curr = null;
            Node<T> potential = null;

            int counter = 0;
            float pathCost = float.MaxValue;
            bool[] visited = new bool[graphSize + 1];
            System.Random random = new System.Random();

            // choose a random starting node
            int index = random.Next(0, graphSize);
            visited[index + 1] = true;
            counter++;
            Debug.Log("Size: " + graphSize);
            Debug.Log("Index Generated: " + index);
            curr = nodes[index];
            tree.Add(curr);

            while (counter != graphSize)
            {
                pathCost = float.MaxValue;
                potential = null;

                foreach (Node<T> neighbour in curr.neighbours)
                {
                    if (!visited[neighbour.id])
                    {
                        //Debug.Log(string.Format("Path cost from node {0} to {1}: {2:0.00}", curr.id, neighbour.id, adjMatrix[curr.id, neighbour.id]));
                        if (adjMatrix[curr.id, neighbour.id] < pathCost)
                        {
                            pathCost = adjMatrix[curr.id, neighbour.id];
                            potential = neighbour;
                        }
                        //this.printDebugGraph();
                    }
                }

                if (potential != null)
                {
                    if (tree.Contains(potential)) break;

                    visited[potential.id] = true;
                    counter++;
                    curr = potential;
                    tree.Add(curr);
                }
                else
                {
                    //Debug.Log("No node left to pick, pick one that hasn't been visited.");
                    for (int i = 1; i < graphSize + 1; i++)
                    {
                        if (!visited[i])
                        {
                            curr = nodes[i - 1];
                            tree.Add(curr);
                            visited[curr.id] = true;
                            counter++;

                            //Debug.Log("Rogue node has id: " + curr.id);
                        }
                    }
                }

                //Debug.Log("Added node with id: " + curr.id);
                //Debug.Log("Tree now has " + tree.Count + " nodes.");
                //Debug.Log(counter + " out of " + graphSize + " rooms added to tree.");
            }

            return tree;
        }
    }
}