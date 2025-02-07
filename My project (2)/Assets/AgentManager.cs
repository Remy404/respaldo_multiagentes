using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class AgentManager : MonoBehaviour
{
    public GameObject pedestrianPrefabP; // Prefab para peatones con ID que empieza con "P"
    public GameObject pedestrianPrefabB; // Prefab para peatones con ID que empieza con "b"
    public GameObject carPrefab; // Prefab para coches
    public GameObject trafficLightPrefab; // Prefab para semáforos

    private Dictionary<string, GameObject> agents = new Dictionary<string, GameObject>(); // Diccionario para agentes (peatones, coches y semáforos)

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            byte[] data = new byte[4096];
            int bytes = stream.Read(data, 0, data.Length);
            string json = Encoding.UTF8.GetString(data, 0, bytes);
            UpdateAgents(json);
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432); // Cambia la dirección y puerto según tu configuración
            stream = client.GetStream();
            Debug.Log("Conectado al servidor.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al conectar al servidor: " + e.Message);
        }
    }

    void UpdateAgents(string json)
{
    Debug.Log("JSON recibido: " + json);

    try
    {
        AgentDataWrapper wrapper = JsonUtility.FromJson<AgentDataWrapper>(json);
        List<string> agentsToRemove = new List<string>(agents.Keys);

        foreach (var agentData in wrapper.agents)
        {
            string agentId = agentData.id;
            Vector3 newPosition = new Vector3(agentData.position.x, agentData.position.y, agentData.position.z);

            if (agents.ContainsKey(agentId))
            {
                GameObject agent = agents[agentId];
                Vector3 currentPosition = agent.transform.position;
                Vector3 direction = (newPosition - currentPosition).normalized;

                // **Rotación instantánea hacia la dirección del movimiento**
                if (direction != Vector3.zero && !agentId.StartsWith("S")) // Evita rotar semáforos
                {
                    agent.transform.rotation = Quaternion.LookRotation(direction);
                }

                agent.transform.position = newPosition;

                if (agentId.StartsWith("S"))
                {
                    TrafficLight trafficLight = agent.GetComponent<TrafficLight>();
                    if (trafficLight != null)
                    {
                        trafficLight.SetState(agentData.state);
                    }
                }

                agentsToRemove.Remove(agentId);
            }
            else
            {
                Debug.Log("Instanciando nuevo agente: " + agentId);
                GameObject prefab = GetPrefabForAgent(agentId);
                if (prefab != null)
                {
                    GameObject newAgent = Instantiate(prefab, newPosition, Quaternion.identity);
                    agents[agentId] = newAgent;

                    if (agentId.StartsWith("S"))
                    {
                        TrafficLight trafficLight = newAgent.GetComponent<TrafficLight>();
                        if (trafficLight != null)
                        {
                            trafficLight.SetState(agentData.state);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Prefab no asignado para el agente: " + agentId);
                }
            }
        }

        foreach (var agentId in agentsToRemove)
        {
            Destroy(agents[agentId]);
            agents.Remove(agentId);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError("Error al procesar el JSON: " + e.Message);
    }
}


    GameObject GetPrefabForAgent(string agentId)
    {
        if (agentId.StartsWith("P"))
        {
            return pedestrianPrefabP; // Prefab para peatones con ID que empieza con "P"
        }
        else if (agentId.StartsWith("b"))
        {
            return pedestrianPrefabB; // Prefab para peatones con ID que empieza con "b"
        }
        else if (agentId.StartsWith("C"))
        {
            return carPrefab; // Prefab para coches
        }
        else if (agentId.StartsWith("S"))
        {
            return trafficLightPrefab; // Prefab para semáforos
        }
        return null; // No hay prefab para otros tipos de agentes
    }

    void OnDestroy()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}

// Clases para representar el JSON
[System.Serializable]
public class AgentPosition
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class AgentData
{
    public string id; // ID del agente (ejemplo: "P0", "b0", "C1", "S0")
    public AgentPosition position;
    public bool state; // Estado del semáforo (solo aplica para semáforos)
}

[System.Serializable]
public class AgentDataWrapper
{
    public List<AgentData> agents;
}