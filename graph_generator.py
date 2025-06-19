import os
import re
from pathlib import Path
import matplotlib.pyplot as plt

def analizuj_pliki_xml(folder_path_str: str):
    """
    Analizuje pliki .xml w podanym folderze, wczytując je w kolejności
    od najstarszego do najnowszego. Dla każdego pliku oblicza średnią
    wartość atrybutu 'fitness'.

    Args:
        folder_path_str (str): Ścieżka do folderu zawierającego pliki .xml.
    """
    
    # --- Krok 1: Walidacja ścieżki i wczytanie plików ---
    
    # Używamy modułu pathlib dla wygodniejszej pracy ze ścieżkami
    folder_path = Path(folder_path_str)

    if not folder_path.is_dir():
        print(f"Błąd: Podana ścieżka '{folder_path_str}' nie istnieje lub nie jest folderem.")
        return

    print(f"Przeszukuję folder: {folder_path.resolve()}")

    # Wyszukujemy wszystkie pliki z rozszerzeniem .xml w podanym folderze
    # i od razu sortujemy je według czasu modyfikacji (od najstarszego do najnowszego)
    # key=os.path.getmtime lub p.stat().st_mtime zwraca czas ostatniej modyfikacji pliku
    try:
        pliki_xml = sorted(
            folder_path.glob('*.xml'),
            key=lambda p: p.stat().st_mtime
        )
    except OSError as e:
        print(f"Wystąpił błąd podczas dostępu do plików: {e}")
        return

    if not pliki_xml:
        print("W podanym folderze nie znaleziono żadnych plików .xml.")
        return

    print(f"Znaleziono {len(pliki_xml)} plików .xml. Przetwarzam w kolejności chronologicznej...")

    # --- Krok 2: Przetwarzanie każdego pliku po kolei ---
    
    # Tablica (lista w Pythonie) do przechowywania rezultatów z każdego pliku
    rezultaty = []

    for plik_path in pliki_xml:
        # Zgodnie z pseudokodem, dla każdego pliku resetujemy liczniki
        suma_fitness = 0.0  # Używamy liczby zmiennoprzecinkowej dla precyzji
        licznik_fitness = 0
        
        print(f"\n--- Analizuję plik: {plik_path.name} ---")

        try:
            # Otwieramy plik do odczytu ('r') z kodowaniem 'utf-8', które jest standardem dla XML
            # Konstrukcja 'with open(...)' automatycznie zamyka plik po zakończeniu bloku
            with plik_path.open('r', encoding='utf-8') as plik:
                # dla każdej linijki:
                for numer_linii, linia in enumerate(plik, 1):
                    # sprawdzamy czy znajduje się fragment fitness="XXX"
                    # Używamy wyrażeń regularnych (moduł re) do znalezienia wzorca
                    # r'fitness="(\d+\.?\d*)"'
                    #   fitness="   - dopasowuje dosłowny tekst
                    #   ( ... )     - tworzy grupę przechwytującą (to, co chcemy wyciągnąć)
                    #   \d+         - dopasowuje jedną lub więcej cyfr
                    #   \.?         - dopasowuje opcjonalną kropkę (dla liczb dziesiętnych)
                    #   \d* - dopasowuje zero lub więcej cyfr po kropce
                    #   "           - dopasowuje cudzysłów zamykający
                    dopasowanie = re.search(r'fitness="(\d+\.?\d*)"', linia)
                    
                    if dopasowanie:
                        # Jeśli znaleziono wzorzec, wyciągamy wartość z grupy 1
                        wartosc_str = dopasowanie.group(1)
                        try:
                            # dodaj do licznika 1, a do zmiennej wartość
                            wartosc_liczba = float(wartosc_str)
                            suma_fitness += wartosc_liczba
                            licznik_fitness += 1
                        except ValueError:
                            print(f"  [Ostrzeżenie] W linii {numer_linii} znaleziono wartość, której nie można przekonwertować na liczbę: {wartosc_str}")

            # Po przeanalizowaniu całego pliku, obliczamy średnią
            if licznik_fitness > 0:
                srednia = suma_fitness / licznik_fitness
                # do tablicy rezultatów zapisz średnią
                wynik_pliku = {'plik': plik_path.name, 'srednia_fitness': srednia, 'znaleziono_wpisow': licznik_fitness}
                rezultaty.append(wynik_pliku)
                print(f"  -> Znaleziono {licznik_fitness} wartości. Średnia fitness = {srednia:.4f}")
            else:
                wynik_pliku = {'plik': plik_path.name, 'srednia_fitness': 0, 'znaleziono_wpisow': 0}
                rezultaty.append(wynik_pliku)
                print("  -> W tym pliku nie znaleziono żadnych atrybutów 'fitness'.")

        except IOError as e:
            print(f"  [Błąd] Nie można otworzyć lub odczytać pliku {plik_path.name}: {e}")
        except Exception as e:
            print(f"  [Błąd] Wystąpił nieoczekiwany błąd podczas przetwarzania pliku {plik_path.name}: {e}")
            
    # --- Krok 3: Wyświetlenie końcowych rezultatów ---
    print("\n\n=== Podsumowanie analizy ===")
    if rezultaty:
        for wynik in rezultaty:
            print(f"Plik: {wynik['plik']:<30} | Średnia fitness: {wynik['srednia_fitness']:.4f} | Liczba wpisów: {wynik['znaleziono_wpisow']}")
    else:
        print("Nie przetworzono żadnych plików.")
    return rezultaty

def narysuj_wykres(rezultaty: list):
    """
    Rysuje automatycznie skalowany wykres liniowy na podstawie listy rezultatów.
    Oś X: Numer pliku (1 do n).
    Oś Y: Średnia wartość 'fitness'.
    
    Args:
        rezultaty (list): Lista słowników z wynikami.
    """
    if not rezultaty:
        print("Brak danych do narysowania wykresu.")
        return

    # Oś Y to wartości 'srednia_fitness'
    srednie_wartosci = [wynik['srednia_fitness'] for wynik in rezultaty]
    
    # Oś X to numer kolejny pliku (1, 2, 3, ..., n)
    numery_plikow = range(1, len(srednie_wartosci) + 1)
    
    # --- Tworzenie wykresu ---
    plt.style.use('seaborn-v0_8-whitegrid')
    fig, ax = plt.subplots(figsize=(12, 7))

    # Rysujemy linię. Używamy mniejszych znaczników (markerów),
    # aby wykres był czytelny przy dużej liczbie punktów.
    ax.plot(numery_plikow, srednie_wartosci, marker='o', linestyle='-', markersize=4, label='Średnia "fitness"')

    # --- Ustawianie etykiet i tytułu ---
    ax.set_title('Średnia wartość "fitness" w kolejnych plikach', fontsize=16)
    ax.set_ylabel('Średnia wartość "fitness"', fontsize=12)
    # Zmieniony tytuł osi X
    ax.set_xlabel('Numer pliku w kolejności chronologicznej (1 = najstarszy)', fontsize=12)

    # --- KLUCZOWA ZMIANA: Automatyczne skalowanie osi X ---
    # Nie ustawiamy etykiet (`xticks`) ręcznie. Matplotlib automatycznie
    # wybierze odpowiednią liczbę i rozmieszczenie etykiet na osi,
    # aby zachować czytelność, niezależnie od liczby danych.
    # Na przykład dla 1500 plików pokaże etykiety co 200, a dla 50 co 5 lub 10.
    
    ax.legend()
    # Włączamy siatkę dla obu osi dla lepszej orientacji
    ax.grid(True, which='both', linestyle='--', linewidth=0.5)
    fig.tight_layout()

    plt.show()

# --- Uruchomienie programu ---
# WAŻNE: Zmień "XYZ" na rzeczywistą ścieżkę do Twojego folderu.
# Przykłady ścieżek:
# W systemie Windows: "C:/Uzytkownicy/TwojaNazwa/Dokumenty/DaneXML"
# W systemie Linux/macOS: "/home/uzytkownik/dokumenty/dane_xml"

sciezka_do_folderu = "D:/Repozytoria/Modelowanie-i-Symulacja-Systemow/Master/src/Trainer/bin/Debug/net8.0/Generation/Old"
rezultaty = analizuj_pliki_xml(sciezka_do_folderu)
narysuj_wykres(rezultaty)
