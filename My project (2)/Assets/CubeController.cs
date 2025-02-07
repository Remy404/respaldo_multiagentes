using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class CubeController : MonoBehaviour
{
    private TcpClient client; // Cliente TCP
    private NetworkStream stream; // Flujo de datos

    private Thread receiveThread; // Hilo para recepción de datos
    private bool isRunning = true; // Control del hilo

    private Vector3 newPosition = Vector3.zero; // Nueva posición para el cubo
    private bool positionUpdated = false; // Bandera para indicar si hay datos nuevos

    void Start()
    {
        // Intentar conectarse al servidor
        try
        {
            client = new TcpClient("127.0.0.1", 65432); // Dirección y puerto del servidor Python
            stream = client.GetStream();

            // Iniciar el hilo de recepción
            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();
            Debug.Log("Conectado al servidor.");
        }
        catch (SocketException ex)
        {
            Debug.LogError("Error al conectar con el servidor: " + ex.Message);
        }
    }

    void Update()
    {
        // Si hay una nueva posición, actualizar la posición del cubo
        if (positionUpdated)
        {
            transform.position = newPosition;
            positionUpdated = false; // Resetear la bandera
        }
    }

    void ReceiveData()
    {
        while (isRunning)
        {
            if (stream != null && stream.DataAvailable)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Debug.Log("Datos recibidos: " + data); // Mostrar los datos recibidos en la consola

                // Parsear los datos JSON
                try
                { 

                    var position = JsonUtility.FromJson<PositionData>(data);
                    
                    Debug.Log("Posición recibida: " + position);
                    newPosition = new Vector3(position.x, position.y, position.z);
                    positionUpdated = true; // Indicar que hay datos nuevos
                }
                catch
                {
                    Debug.LogError("Error al parsear los datos: " + data);
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        // Cerrar conexiones y detener el hilo
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }

    // Clase para mapear los datos JSON
    [System.Serializable]
    public class PositionData
    {
        public float x;
        public float y;
        public float z;
    }
}