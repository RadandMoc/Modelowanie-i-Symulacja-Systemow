import neat
import socket
import threading
import os

HOST = '127.0.0.1'  # adres Unity (dla wysyłania poleceń)
UNITY_PORT = 5005   # port, na którym Unity nasłuchuje

PYTHON_LISTEN_PORT = 6006  # port, na którym Python nasłuchuje wiadomości od Unity

def interpret_network_output(output):
    """
    Na podstawie wyjścia sieci NEAT zwraca polecenie dla drona.
    Zakładamy, że output to lista wartości liczbowych.
    """
    # Przykładowa logika interpretacji:
    if output[0] > 0.5:
        command = "FORWARD"
    else:
        command = "BACKWARD"
    # Możesz dodać więcej warunków dla innych ruchów.
    return command

def evaluate_network(net):
    # Przykładowy stan wejściowy; w rzeczywistości stan powinien pochodzić z symulacji Unity lub być zdefiniowany na podstawie wymagań.
    state = [0.1, 0.2, 0.3]
    output = net.activate(state)
    print("NEAT network output:", output)
    return interpret_network_output(output)

def send_drone_command(command):
    """
    Wysyła polecenie do Unity jako klient TCP.
    """
    try:
        with socket.create_connection((HOST, UNITY_PORT)) as s:
            message = command + "\n"
            s.sendall(message.encode('utf-8'))
            print(f"[PYTHON] Wysłano polecenie: {command}")
            # Opcjonalnie: odbieramy odpowiedź (np. potwierdzenie lub wartość fitness)
            response = s.recv(1024).decode('utf-8').strip()
            print(f"[PYTHON] Otrzymano odpowiedź: {response}")
    except Exception as e:
        print(f"[PYTHON] Błąd podczas wysyłania polecenia: {e}")

def start_python_server():
    """
    Uruchamia serwer TCP w Pythonie, który nasłuchuje na wiadomości od Unity.
    """
    server_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_sock.bind(('127.0.0.1', PYTHON_LISTEN_PORT))
    server_sock.listen(1)
    print(f"[PYTHON] Python server nasłuchuje na porcie {PYTHON_LISTEN_PORT}")
    while True:
        conn, addr = server_sock.accept()
        print(f"[PYTHON] Połączono z Unity z adresu: {addr}")
        threading.Thread(target=handle_unity_connection, args=(conn,), daemon=True).start()

def handle_unity_connection(conn):
    """
    Obsługuje pojedyncze połączenie od Unity.
    """
    with conn:
        while True:
            data = conn.recv(1024)
            if not data:
                break
            message = data.decode('utf-8').strip()
            print(f"[PYTHON] Odebrano z Unity: {message}")

def run_neat_and_send_command(config_file):
    """
    Przykładowa funkcja, która tworzy sieć NEAT, interpretuje jej wyjście i wysyła polecenie do Unity.
    """
    config = neat.Config(neat.DefaultGenome, neat.DefaultReproduction,
                         neat.DefaultSpeciesSet, neat.DefaultStagnation,
                         config_file)
    p = neat.Population(config)
    
    genomes = list(p.population.items())
    if genomes:
        genome_id, genome = genomes[0]
        net = neat.nn.FeedForwardNetwork.create(genome, config)
        command = evaluate_network(net)
        send_drone_command(command)
    else:
        print("Brak genomów w populacji.")

if __name__ == '__main__':
    # Uruchom serwer w osobnym wątku, aby Python mógł nasłuchiwać Unity
    threading.Thread(target=start_python_server, daemon=True).start()

    # Ścieżka do pliku konfiguracyjnego NEAT (np. neat-config.txt)
    local_dir = os.path.dirname(__file__)
    config_path = os.path.join(local_dir, 'neat-config.txt')
    
    # Uruchom przykładową ewolucję i wyślij polecenie do Unity
    run_neat_and_send_command(config_path)
