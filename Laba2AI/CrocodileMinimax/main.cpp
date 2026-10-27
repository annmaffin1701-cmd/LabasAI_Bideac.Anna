#include <algorithm>
#include <cstdlib>
#include <iostream>
#include <limits>
#include <vector>

constexpr int INITIAL_TEETH = 21;
constexpr int MAX_MOVE = 2;
constexpr int SEARCH_DEPTH_LIMIT = 21;

constexpr int WIN_SCORE = 100;
constexpr int HEURISTIC_SCORE = 10;

struct GameState {
    int remainingTeeth;
    bool aiTurn;  // Ход ИИ
};

//Правила 

std::vector<int> getValidMoves(const GameState& state) {
    std::vector<int> moves;
    for (int count = 1; count <= std::min(MAX_MOVE, state.remainingTeeth); ++count) {
        moves.push_back(count);
    }
    return moves;
}

GameState applyMove(const GameState& state, int move) {
    return {state.remainingTeeth - move, !state.aiTurn};
}

bool isTerminal(const GameState& state) {
    return state.remainingTeeth == 0;
}

// Оценка позиции с точки зрения ИИ (больше - лучше для ИИ).
int evaluate(const GameState& state) {
    if (isTerminal(state)) {
        // Ход перешёл к тому, кто НЕ нажал последний зуб, значит он победил.
        return state.aiTurn ? WIN_SCORE : -WIN_SCORE;
    }

    // Проигрышная позиция для того, кто ходит: остаток % 3 == 1.
    const bool currentPlayerLoses = state.remainingTeeth % (MAX_MOVE + 1) == 1;
    const bool goodForAi = currentPlayerLoses != state.aiTurn;
    return goodForAi ? HEURISTIC_SCORE : -HEURISTIC_SCORE;
}

// ИИ 

int minimax(const GameState& state, int depth, bool isMaximizing, long long& nodes) {
    ++nodes;

    if (isTerminal(state) || depth == 0) {
        return evaluate(state);
    }

    int bestScore = isMaximizing ? std::numeric_limits<int>::min()
                                 : std::numeric_limits<int>::max();

    for (int move : getValidMoves(state)) {
        const int score = 1imax(applyMove(state, move), depth - 1, !isMaximizing, nodes);
        bestScore = isMaximizing ? std::max(bestScore, score)
                                 : std::min(bestScore, score);
    }
    return bestScore;
}

int chooseBestMove(const GameState& state) {
    int bestMove = -1;
    int bestScore = std::numeric_limits<int>::min();
    long long nodes = 0;

    std::cout << "\nAI is analyzing the game tree:\n";
    for (int move : getValidMoves(state)) {
        const GameState next = applyMove(state, move);
        const int depth = std::min(SEARCH_DEPTH_LIMIT - 1, next.remainingTeeth);
        const int score = minimax(next, depth, false, nodes);

        std::cout << "  Press " << move << " tooth/teeth: score " << score << '\n';
        if (score > bestScore) {
            bestScore = score;
            bestMove = move;
        }
    }

    std::cout << "Positions analyzed: " << nodes << '\n';
    return bestMove;
}

//  Ввод / вывод 

int readChoice(int minValue, int maxValue) {
    while (true) {
        int value;
        if (std::cin >> value && value >= minValue && value <= maxValue) {
            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
            return value;
        }

        if (std::cin.eof()) {
            std::exit(0);  // ввод закрыт - выходим, а не крутимся в цикле
        }

        std::cin.clear();
        std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
        std::cout << "Enter a number from " << minValue << " to " << maxValue << ": ";
    }
}

void printMouth(const GameState& state) {
    std::cout << "\n      CROCODILE'S MOUTH:\n"
              << "   /---------------------------------------------------------------------\\\n"
              << "  | ";
    for (int i = 0; i < INITIAL_TEETH; ++i) {
        std::cout << (i < state.remainingTeeth ? "[#]" : "[X]");
    }
    std::cout << " |\n"
              << "   \\---------------------------------------------------------------------/\n"
              << "Teeth left: " << state.remainingTeeth << '\n';
}


int askPlayerMove(const GameState& state) {
    const int maxMove = std::min(MAX_MOVE, state.remainingTeeth);
    std::cout << "Your turn. Press 1" << (maxMove == 2 ? " or 2" : "") << " tooth/teeth: ";
    return readChoice(1, maxMove);
}

void printResult(const GameState& state) {
    // На конце игры aiTurn == true значит последний зуб нажал человек.
    if (state.aiTurn) {
        std::cout << "\nThe crocodile bites! You pressed the last tooth.\n"
                  << "YOU LOSE. AI WINS!!! HAHAHAH\n";
    } else {
        std::cout << "\nThe crocodile bites the AI! It pressed the last tooth.\n"
                  << "YOU WIN. AI LOSES!!!\n";
    }
}

// Main 

int main() {
    std::cout << "CROCODILE TEETH.\n"
              << "There are " << INITIAL_TEETH
              << " teeth. Press 1 or 2 teeth per turn.\n"
              << "Whoever presses the LAST tooth LOSES.\n\n"
              << "Who plays first? 1 - You, 2 - AI: ";

    bool aiStarts = readChoice(1, 2) == 2;

    while (true) {
        // Начало новой партии
        GameState state{INITIAL_TEETH, aiStarts};

        std::cout << "\n NEW GAME \n"
                  << "First turn: "
                  << (aiStarts ? "AI" : "You") << "\n";

        // Основной игровой цикл
        while (!isTerminal(state)) {
            printMouth(state);

            int move;

            if (state.aiTurn) {
                move = chooseBestMove(state);

                std::cout << "AI presses " << move
                          << " tooth/teeth.\n";
            } else {
                move = askPlayerMove(state);

                std::cout << "You press " << move
                          << " tooth/teeth.\n";
            }

            state = applyMove(state, move);
        }

        printMouth(state);
        printResult(state);

        // Предложение начать новую партию
        std::cout << "\nPlay again? 1 - Yes, 2 - No,exit: ";

        int restart = readChoice(1, 2);

        if (restart == 2) {
            std::cout << "Thanks for playing!\n";
            break;
        }

        // Меняем первого игрока для следующей партии
        aiStarts = !aiStarts;
    }


    return 0;
}
