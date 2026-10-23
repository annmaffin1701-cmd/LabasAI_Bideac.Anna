#include <algorithm>
#include <cstdlib>
#include <iostream>
#include <limits>
#include <vector>

constexpr int INITIAL_TEETH = 7;
constexpr int MAX_MOVE = 2;
constexpr int SEARCH_DEPTH_LIMIT = 7;

constexpr int WIN_SCORE = 100;
constexpr int HEURISTIC_SCORE = 10;

struct GameState {
    int remainingTeeth;
    bool aiTurn; // true: AI moves next; false: human moves next
};

// List of legal moves: press one or two teeth (if available).
std::vector<int> getValidMoves(const GameState& state) {
    std::vector<int> moves;
    for (int count = 1; count <= std::min(MAX_MOVE, state.remainingTeeth); ++count) {
        moves.push_back(count);
    }
    return moves;
}

// Return a NEW state after a move, switching the active player.
GameState applyMove(const GameState& state, int move) {
    return {state.remainingTeeth - move, !state.aiTurn};
}

bool isTerminal(const GameState& state) {
    return state.remainingTeeth == 0;
}

// Positive scores favor AI, negative scores favor the human.
int evaluate(const GameState& state) {
    if (isTerminal(state)) {
        // The player who just pressed the final tooth loses.
        // The turn has already switched, so state.aiTurn means the human lost.
        return state.aiTurn ? WIN_SCORE : -WIN_SCORE;
    }

    // In this game, n % 3 == 1 is losing for the player whose turn it is.
    const bool activePlayerLoses =
        state.remainingTeeth % (MAX_MOVE + 1) == 1;
    const bool goodForAi = activePlayerLoses != state.aiTurn;
    return goodForAi ? HEURISTIC_SCORE : -HEURISTIC_SCORE;
}

// Maximize on AI turns, minimize on human turns.
int minimax(const GameState& state, int depth, bool isMaximizing,
            long long& nodes) {
    ++nodes;

    if (isTerminal(state) || depth == 0) {
        return evaluate(state);
    }

    int bestScore = isMaximizing ? std::numeric_limits<int>::min()
                                 : std::numeric_limits<int>::max();

    for (int move : getValidMoves(state)) {
        const GameState next = applyMove(state, move);
        const int score = minimax(next, depth - 1, !isMaximizing, nodes);
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
        const int depth = std::min(SEARCH_DEPTH_LIMIT - 1,
                                   next.remainingTeeth);
        const int score = minimax(next, depth, false, nodes);

        std::cout << "  Press " << move << " tooth/teeth: score "
                  << score << '\n';
        if (score > bestScore) {
            bestScore = score;
            bestMove = move;
        }
    }

    std::cout << "Positions analyzed: " << nodes << '\n';
    return bestMove;
}

// Read a bounded integer, retrying on invalid input.
int readChoice(int minValue, int maxValue) {
    while (true) {
        int value;
        if (std::cin >> value && value >= minValue && value <= maxValue) {
            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
            return value;
        }

        if (std::cin.eof()) {
            std::exit(0);
        }

        std::cin.clear();
        std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
        std::cout << "Enter a number from " << minValue << " to "
                  << maxValue << ": ";
    }
}

void printMouth(const GameState& state) {
    std::cout << "\n    CROCODILE'S MOUTH:\n"
              << "  /-------------------------\\\n"
              << " | ";
    for (int i = 0; i < INITIAL_TEETH; ++i) {
        std::cout << (i < state.remainingTeeth ? "[#]" : "[X]");
    }
    std::cout << " |\n"
              << "  \\-------------------------/\n"
              << "Teeth left: " << state.remainingTeeth << '\n';
}

int askPlayerMove(const GameState& state) {
    const int maxMove = std::min(MAX_MOVE, state.remainingTeeth);
    std::cout << "Your turn. Press 1"
              << (maxMove == 2 ? " or 2" : "")
              << " tooth/teeth: ";
    return readChoice(1, maxMove);
}

void printResult(const GameState& state) {
    if (state.aiTurn) {
        std::cout << "\nThe crocodile bites! You pressed the last tooth.\n"
                  << "YOU LOSE. AI WINS!\n";
    } else {
        std::cout << "\nThe crocodile bites the AI! It pressed the last tooth.\n"
                  << "YOU WIN. AI LOSES!\n";
    }
}

int main() {
    std::cout << "CROCODILE TEETH.\n"
              << "There are " << INITIAL_TEETH
              << " teeth. Press 1 or 2 teeth per turn.\n"
              << "Whoever presses the LAST tooth LOSES.\n\n"
              << "Who plays first? 1 - You, 2 - AI: ";

    GameState state{INITIAL_TEETH, readChoice(1, 2) == 2};

    while (!isTerminal(state)) {
        printMouth(state);

        int move;
        if (state.aiTurn) {
            move = chooseBestMove(state);
            std::cout << "AI presses " << move << " tooth/teeth.\n";
        } else {
            move = askPlayerMove(state);
            std::cout << "You press " << move << " tooth/teeth.\n";
        }
        state = applyMove(state, move);
    }

    printMouth(state);
    printResult(state);
    return 0;
}
