<template>
    <div id="root">
        <div v-if="state==='Loading'">Loading...</div>
        <div v-if="state==='Error'">Invalid or expired game</div>
        <div v-if="state==='Ready'">🕐 Waiting for game to start 🕐</div>
        <div v-if="guessGrid.length" class="guess-grid" :style="`width:calc(${1.5*wordLetterCount}em + ${2*4*wordLetterCount}px)`">
            <div v-for="guess of guessGrid">
                <div v-for="guessletter in guess" :class="`guess-letter guess-letter-${guessletter.state.toLowerCase()}`">{{guessletter.character}}</div>
            </div>
            <div v-if="guessTimePercent != null" class="guess-progress" :class="{ hurry: guessTimeHurry }"><div :style="`width:${guessTimePercent}%`"></div></div>
        </div>
        <div v-if="state==='Playing'" class="candidates">
            <div v-if="guessCandidates.length==0">Be the first to guess a word!</div>
            <template v-for="candidate of guessCandidates">
                <div :class="{ minor: candidate.isMinor }">{{candidate.word}} ({{candidate.count}})</div> <!--todo button to allow quick guess-->
            </template>
        </div>
        <div v-if="state==='Win'">🎈 Yeah! 🎈</div>
        <div v-if="state==='Loss'">💩 Game Over 💩</div>
    </div>
</template>

<style scoped>
    #root {
        font-size: 2em;
        text-align: center;
        overflow: auto;
    }
    .guess-grid {
        margin-left: auto;
        margin-right: auto;
    }
    .candidates {
        font-size: 80%;
    }
    .candidates div {
        display: inline-block;
        margin: 15px;
    }
    .candidates .minor {
        font-size: 80%;
    }
</style>

<script>
    export default {
        props: {
            publicToken: String, //todo handle changes after mount()
            guessWord: String,
        },

        emits: {
            ready(length, maxGuesses) { return Number.isInteger(length) && Number.isInteger(maxGuesses) },
            playing() { return true },
            guess(letterStates) { return typeof letterStates === 'object' },
            finished() { return true },
        },

        data() {
            return {
                state: 'Loading',
                readyEmitted: false,
                playingEmitted: false,
                guessEmittedAtCount: 0,
                finishedEmitted: false,
                wordLetterCount: null,
                guessGrid: [],
                guessStartUTC: null,
                guessEndUTC: null,
                guessTimePercent: null,
                guessTimeHurry: false,
                guessCandidates: [],
                connection: null,
                timerId: null,
            }
        },

        methods: {
            updateGameStateWrapped(summary) {
                try {
                    this.updateGameState(summary)
                } catch (ex) {
                    console.error(ex) // otherwise signalr swallows the exception
                }
            },

            updateGameState(summary) {
                const grid = summary.pastGuesses
                const wordLength = summary.length
                this.wordLetterCount = wordLength
                this.emitReadyEventOnce(summary.length, summary.maxGuesses)
                this.emitGuessEventEachGuess(summary.pastGuesses)

                if (summary.state === 'Playing') {

                    this.emitPlayingEventOnce()

                    grid.push(this.wordToRow(this.guessWord, wordLength, 'initial'))

                    this.guessCandidates = summary.currentGuess.guessCandidates
                    this.sortAndAugmentCandidates()

                    this.guessStartUTC = Date.parse(summary.currentGuess.startTimeUTC)
                    this.guessEndUTC = Date.parse(summary.currentGuess.endTimeUTC)
                    this.startTimer()

                } else {
                    this.guessCandidates = []
                    this.guessStartUTC = null
                    this.guessEndUTC = null
                    this.guessTimePercent = null
                    this.stopTimer()
                }

                if (summary.state === 'Loss') {
                    // Show answer as the final entry.
                    const answerRow = this.wordToRow(summary.answer, summary.answer.length, 'answer')
                    grid.push(answerRow)
                }

                if (summary.state === 'Win' || summary.state === 'Loss') {
                    this.emitFinishedEventOnce()
                }

                this.state = summary.state
                this.guessGrid = grid
            },

            emitReadyEventOnce(wordLength, maxGuesses) {
                if (!this.readyEmitted) {
                    this.$emit('ready', wordLength, maxGuesses)
                    this.readyEmitted = true;
                }
            },

            emitPlayingEventOnce() {
                if (!this.playingEmitted) {
                    this.$emit('playing')
                    this.playingEmitted = true;
                }
            },

            emitGuessEventEachGuess(pastGuesses) {
                if (pastGuesses.length != this.guessEmittedAtCount) {
                    // Aggregate all letter states across all guesses to a single letter:state map.
                    // Precedence of states is: Correct, Present, Absent
                    // ie a letter is 'Correct' if it is correct anywhere in the guess history,
                    // even if it is 'Present' or 'Absent' in later guesses.
                    const precedence = ['Correct', 'Present', 'Absent', undefined]
                    var letterStates = {}
                    for (const guessWord of pastGuesses) {
                        for (const guessLetter of guessWord) {
                            const p1 = precedence.indexOf(letterStates[guessLetter.character])
                            const p2 = precedence.indexOf(guessLetter.state)
                            if (p2 < p1) {
                                letterStates[guessLetter.character] = guessLetter.state
                            }
                        }
                    }
                    this.$emit('guess', letterStates)
                    this.guessEmittedAtCount = pastGuesses.length
                }
            },

            emitFinishedEventOnce() {
                if (!this.finishedEmitted) {
                    this.$emit('finished')
                    this.finishedEmitted = true;
                }
            },

            wordToRow(word, length, state) {
                word = (word || '').toUpperCase().padEnd(length, ' ').substring(0, length);
                const row = word.split('').map(function (c) { return { character: c, state: state } })
                return row
            },

            sortAndAugmentCandidates() {
                this.guessCandidates.sort(function (c1, c2) { return c2.count - c1.count }) // sort descending

                var guessCount = 0;
                for (const candidate of this.guessCandidates) {
                    guessCount += candidate.count
                }
                const average = guessCount / Math.max(this.guessCandidates.length, 1)

                for (const candidate of this.guessCandidates) {
                    candidate.isMinor = candidate.count < average || candidate.count == 1
                }
            },

            startTimer() {
                if (this.timerId == null) {
                    this.timerId = setInterval(this.timerUpdate, 50)
                }
            },

            stopTimer() {
                if (this.timerId != null) {
                    clearInterval(this.timerId)
                }
            },

            timerUpdate() {
                if (this.guessStartUTC && this.guessEndUTC) {
                    const percent = (Date.now() - this.guessStartUTC) / (this.guessEndUTC - this.guessStartUTC) * 100
                    this.guessTimePercent = Math.min(Math.max(percent, 0), 100) // clamp to 0-100
                }

                this.guessTimeHurry = this.guessTimePercent > 80
            },
        },

        expose: [],

        watch: {
            guessWord() {
                if (this.state === 'Playing') {
                    const grid = this.guessGrid
                    grid.pop()
                    grid.push(this.wordToRow(this.guessWord, this.wordLetterCount, 'initial'))
                    this.guessGrid = grid

                    if (this.connection) {
                        // Un-guess if word length is not the game word length.
                        const isGuessCorrectLength = (this.guessWord || '').length == this.wordLetterCount
                        const guessWord = isGuessCorrectLength ? this.guessWord : null
                        this.connection.invoke("GuessWord", guessWord).catch(function (err) {
                            return console.error(err.toString());
                        });
                    }
                }
            },
        },

        async mounted() {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/game/" + this.publicToken)
                .withAutomaticReconnect()
                .build()
            this.connection.on("GameState", this.updateGameStateWrapped)
            const thiss = this
            this.connection.onclose(function (error) {
                console.error('Invalid game url: ' + error)
                thiss.state = 'Error'
            });
            await this.connection.start()
        },

        async unmounted() {
            if (this.connection) {
                await this.connection.stop()
            }

            this.stopTimer()
        },
    }
</script>
