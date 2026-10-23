#include <algorithm>
#include <iostream>
#include <limits>
#include <vector>

constexpr int INITIAL_TEETH = 7;
constexpr int SEARCH_DEPTH_LIMIT = 7;

struct GameState {
    int remainingTeeth;
    bool aiTurn;
};

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
    if (isTerminal(state)) {
        return state.aiTurn ? 100 : -100;
    }

    bool losingForCurrentPlayer = state.remainingTeeth % 3 == 1;
    if (losingForCurrentPlayer) {
        return state.aiTurn ? -10 : 10;
    }
    return state.aiTurn ? 10 : -10;
}

long long analyzedNodes = 0;

int minimax(const GameState& state, int depth, bool is_maximizing) {
    ++analyzedNodes;

    if (isTerminal(state) || depth == 0) {
        return evaluate(state);
    }

    if (is_maximizing) {
        int bestScore = std::numeric_limits<int>::min();
        for (int move : getValidMoves(state)) {
            GameState nextState = applyMove(state, move);
            int score = minimax(nextState, depth - 1, false);
            bestScore = std::max(bestScore, score);
        }
        return bestScore;
    }

    int bestScore = std::numeric_limits<int>::max();
    for (int move : getValidMoves(state)) {
        GameState nextState = applyMove(state, move);
        int score = minimax(nextState, depth - 1, true);
        bestScore = std::min(bestScore, score);
    }
    return bestScore;
}

int chooseBestMove(const GameState& state) {
    int bestMove = -1;
    int bestScore = std::numeric_limits<int>::min();
    analyzedNodes = 0;

    std::cout << "\nAI is analyzing the game tree:\n";
    for (int move : getValidMoves(state)) {
        GameState nextState = applyMove(state, move);
        int searchDepth = std::min(SEARCH_DEPTH_LIMIT - 1, nextState.remainingTeeth);
        int score = minimax(nextState, searchDepth, false);

        std::cout << "  Press " << move << " tooth/teeth: score " << score << '\n';
        if (score > bestScore) {
            bestScore = score;
            bestMove = move;
        }
    }

    std::cout << "Positions analyzed: " << analyzedNodes << '\n';
    return bestMove;
}

int readChoice(int minValue, int maxValue) {
    while (true) {
        int value;
        if (std::cin >> value && value >= minValue && value <= *maxValue*) {
            std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
            return value;
        }

        std::cin.clear();
        std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
        std::cout << "Enter a number from " << minValue << " to " << maxValue << ": ";
    }
}

void printMouth(const GameState& state) {
    std::cout << "\n      CROCODILE'S MOUTH\n";
    std::cout << "   /-----------------------\\\n";
    std::cout << "  | ";
    for (int index = 0; index < INITIAL_TEETH; ++index) {
        std::cout << (index < state.remainingTeeth ? "[#]" : "[X]");
    }
    std::cout << " |\n";
    std::cout << "   \\-----------------------/\n";
    std::cout << "Teeth left: " << state.remainingTeeth << "\n";
}

int main() {
    std::cout << "CROCODILE TEETH - MINIMAX\n";
    std::cout << "There are 7 teeth. Press 1 or 2 teeth per turn.\n";
    std::cout << "Whoever presses the LAST tooth LOSES.\n\n";
    std::cout << "Who plays first? 1 - You, 2 - AI: ";

    int firstPlayer = readChoice(1, 2);
    GameState state{INITIAL_TEETH, firstPlayer == 2};

    while (!isTerminal(state)) {
        printMouth(state);

        int move;
        if (state.aiTurn) {
            move = chooseBestMove(state);
            std::cout << "AI presses " << move << " tooth/teeth.\n";
        } else {
            int maxMove = std::min(2, state.remainingTeeth);
            std::cout << "Your turn. Press 1";
            if (maxMove == 2) {
                std::cout << " or 2";
            }
            std::cout << " tooth/teeth: ";
            move = readChoice(1, maxMove);
            std::cout << "You press " << move << " tooth/teeth.\n";
        }

        state = applyMove(state, move);
    }

    printMouth(state);
    if (state.aiTurn) {
        std::cout << "\nThe crocodile bites! You pressed the last tooth.\n";
        std::cout << "YOU LOSE. AI WINS.\n";
    } else {
        std::cout << "\nThe crocodile bites the AI! It pressed the last tooth.\n";
        std::cout << "-------YOU WIN. AI LOSES.-------\n";
    }

    return 0;
}
