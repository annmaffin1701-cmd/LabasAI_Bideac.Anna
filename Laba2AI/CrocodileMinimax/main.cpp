#include <algorithm>
#include <iostream>
#include <limits>
#include <vector>
#include <string>

#ifdef _WIN32
#include <windows.h>
#endif

constexpr int INITIAL_TEETH = 7;
constexpr int SEARCH_DEPTH_LIMIT = 7;

struct GameState {
    int remainingTeeth;
    bool aiTurn;
};

// ---------- GAME LOGIC ----------

std::vector<int> getValidMoves(const GameState& state) {
    std::vector<int> moves;

    for (int count = 1; count <= 2; ++count) {
        if (count <= state.remainingTeeth) {
            moves.push_back(count);
        }
    }

    return moves;
}

GameState applyMove(const GameState& state, int move) {
    return {state.remainingTeeth - move, !state.aiTurn};
}

bool isTerminal(const GameState& state) {
    return state.remainingTeeth == 0;
}

int evaluate(const GameState& state) {
    // The player who presses the last tooth loses.
    if (isTerminal(state)) {
        return state.aiTurn ? 100 : -100;
    }

    // Positions with 1, 4 or 7 teeth are losing
    // for the player whose turn it is.
    bool losingPosition = state.remainingTeeth % 3 == 1;

    if (losingPosition) {
        return state.aiTurn ? -10 : 10;
    }

    return state.aiTurn ? 10 : -10;
}

// ---------- MINIMAX AI ----------

long long analyzedNodes = 0;

int minimax(const GameState& state, int depth) {
    ++analyzedNodes;

    if (isTerminal(state) || depth == 0) {
        return evaluate(state);
    }

    // AI maximizes its score.
    if (state.aiTurn) {
        int bestScore = std::numeric_limits<int>::min();

        for (int move : getValidMoves(state)) {
            GameState nextState = applyMove(state, move);
            int score = minimax(nextState, depth - 1);
            bestScore = std::max(bestScore, score);
        }

        return bestScore;
    }

    // Human minimizes the AI's score.
    int bestScore = std::numeric_limits<int>::max();

    for (int move : getValidMoves(state)) {
        GameState nextState = applyMove(state, move);
        int score = minimax(nextState, depth - 1);
        bestScore = std::min(bestScore, score);
    }

    return bestScore;
}

int chooseBestMove(const GameState& state) {
    int bestMove = 1;
    int bestScore = std::numeric_limits<int>::min();

    analyzedNodes = 0;

    for (int move : getValidMoves(state)) {
        GameState nextState = applyMove(state, move);

        int depth = std::min(
            SEARCH_DEPTH_LIMIT - 1,
            nextState.remainingTeeth
        );

        int score = minimax(nextState, depth);

        if (score > bestScore) {
            bestScore = score;
            bestMove = move;
        }
    }

    std::cout << "\n  AI has analyzed "
              << analyzedNodes << " positions.\n";

    return bestMove;
}

// ---------- INPUT ----------

int readChoice(int minValue, int maxValue) {
    int value;

    while (true) {
        if (std::cin >> value &&
            value >= minValue &&
            value <= maxValue) {

            std::cin.ignore(
                std::numeric_limits<std::streamsize>::max(),
                '\n'
            );

            return value;
        }

        std::cin.clear();
        std::cin.ignore(
            std::numeric_limits<std::streamsize>::max(),
            '\n'
        );

        std::cout << "  Enter a number from "
                  << minValue << " to " << maxValue << ": ";
    }
}

// ---------- CONSOLE DESIGN ----------

void printHeader() {
    std::cout << "\n";
    std::cout << "  ======================================\n";
    std::cout << "       CROCODILE TEETH CHALLENGE\n";
    std::cout << "  ======================================\n";
}

void printMenu() {
    printHeader();

    std::cout << "\n";
    std::cout << "       Choose your game mode\n\n";
    std::cout << "       [1]  Player vs AI\n";
    std::cout << "       [2]  Two players\n";
    std::cout << "       [3]  Exit\n\n";
    std::cout << "  --------------------------------------\n";
    std::cout << "  Select: ";
}

void printMouth(const GameState& state) {
    std::cout << "\n";
    std::cout << "  +------------------------------------+\n";
    std::cout << "  |           CROCODILE MOUTH          |\n";
    std::cout << "  +------------------------------------+\n";
    std::cout << "  |                                    |\n";
    std::cout << "  |   ";

    for (int i = 0; i < INITIAL_TEETH; ++i) {
        if (i < state.remainingTeeth) {
            std::cout << " [^] ";
        } else {
            std::cout << " [X] ";
        }
    }

    std::cout << "  |\n";
    std::cout << "  |                                    |\n";
    std::cout << "  +------------------------------------+\n";

    std::cout << "       Teeth remaining: "
              << state.remainingTeeth
              << " / " << INITIAL_TEETH << "\n";

    std::cout << "       ";

    for (int i = 0; i < INITIAL_TEETH; ++i) {
        std::cout << (i < state.remainingTeeth ? "#" : "-");
    }

    std::cout << "\n\n";
}

void printRules() {
    std::cout << "  RULES\n";
    std::cout << "  --------------------------------------\n";
    std::cout << "  * Take 1 or 2 teeth per turn.\n";
    std::cout << "  * Players take turns.\n";
    std::cout << "  * Whoever takes the last tooth loses!\n";
    std::cout << "  --------------------------------------\n";
}

// ---------- GAME ----------

void playGame(int mode) {
    GameState state{INITIAL_TEETH, false};

    printHeader();
    printRules();

    if (mode == 1) {
        std::cout << "\n  Who plays first?\n";
        std::cout << "  [1] You\n";
        std::cout << "  [2] AI\n";
        std::cout << "  Select: ";

        int firstPlayer = readChoice(1, 2);
        state.aiTurn = (firstPlayer == 2);
    } else {
        std::cout << "\n  Who plays first?\n";
        std::cout << "  [1] Player 1\n";
        std::cout << "  [2] Player 2\n";
        std::cout << "  Select: ";

        int firstPlayer = readChoice(1, 2);
        state.aiTurn = (firstPlayer == 2);
    }

    while (!isTerminal(state)) {
        printMouth(state);

        int move = 0;

        if (mode == 1 && state.aiTurn) {
            std::cout << "  AI is thinking...\n";

            move = chooseBestMove(state);

            std::cout << "  AI takes " << move
                      << (move == 1 ? " tooth.\n" : " teeth.\n");
        } else {
            std::string playerName;

            if (mode == 2) {
                playerName = state.aiTurn ? "Player 2" : "Player 1";
            } else {
                playerName = "You";
            }

            int maxMove = std::min(2, state.remainingTeeth);

            std::cout << "  " << playerName << "'s turn.\n";
            std::cout << "  Take 1";

            if (maxMove == 2) {
                std::cout << " or 2";
            }

            std::cout << " teeth: ";

            move = readChoice(1, maxMove);

            std::cout << "  " << playerName << " takes "
                      << move
                      << (move == 1 ? " tooth.\n" : " teeth.\n");
        }

        state = applyMove(state, move);
    }

    printMouth(state);

    std::cout << "\n";
    std::cout << "  ======================================\n";
    std::cout << "               GAME OVER\n";
    std::cout << "  ======================================\n";

    if (mode == 1) {
        if (state.aiTurn) {
            std::cout << "\n  The crocodile bites! You took the last tooth.\n";
            std::cout << "  AI WINS! Better luck next time!\n";
        } else {
            std::cout << "\n  The crocodile bites the AI!\n";
            std::cout << "  YOU WIN! Congratulations!\n";
        }
    } else {
        if (state.aiTurn) {
            std::cout << "\n  The crocodile bites Player 1!\n";
            std::cout << "  PLAYER 2 WINS!\n";
        } else {
            std::cout << "\n  The crocodile bites Player 2!\n";
            std::cout << "  PLAYER 1 WINS!\n";
        }
    }

    std::cout << "  ======================================\n";
}

// ---------- MAIN MENU AND RESTART ----------

int main() {
#ifdef _WIN32
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
#endif

    while (true) {
        printMenu();

        int mode = readChoice(1, 3);

        if (mode == 3) {
            std::cout << "\n  Thanks for playing!\n";
            std::cout << "  Goodbye!\n\n";
            break;
        }

        playGame(mode);

        std::cout << "\n";
        std::cout << "  What would you like to do?\n\n";
        std::cout << "  [1] Play again\n";
        std::cout << "  [2] Return to main menu\n";
        std::cout << "  [3] Exit\n\n";
        std::cout << "  Select: ";

        int choice = readChoice(1, 3);

        if (choice == 1) {
            // Start another game using the same mode.
            playGame(mode);
        } else if (choice == 2) {
            // Return to the main menu.
            continue;
        } else {
            std::cout << "\n  Thanks for playing!\n";
            break;
        }

        // After a replay, offer the menu again.
        std::cout << "\n";
        std::cout << "  [1] Play again\n";
        std::cout << "  [2] Return to main menu\n";
        std::cout << "  [3] Exit\n";
        std::cout << "  Select: ";

        choice = readChoice(1, 3);

        if (choice == 1) {
            continue;
        } else if (choice == 2) {
            continue;
        } else {
            std::cout << "\n  Thanks for playing!\n";
            break;
        }
    }

    return 0;
}