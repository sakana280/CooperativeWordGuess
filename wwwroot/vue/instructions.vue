<template>
    <h1>Instructions:</h1>
    <div class="row">
        <div class="col s12 l6 instruction-card">
            <p>▶ You have {{maxGuesses}} turns to guess a {{maxLength}} letter word.</p>
            <div class="center-align">
                <div class="example-guess">
                    <div v-for="i in maxLength" class="guess-letter guess-letter-initial">{{i <= showBlankLetters ? '?' : ''}}</div>
                    <div class="guess-progress" :class="{ hurry: guessTimePercent > 80 }"><div :style="`width:${guessTimePercent}%`"></div></div>
                </div>
            </div>
            <p>▶ Each turn is time-limited, as shown by the animated countdown bar.</p>
            <p>▶ Type in your own guess, or up-vote someone else's guess by clicking on it.</p>
        </div>
        <div class="col s12 l6 instruction-card">
            <p>▶ When the countdown completes for a turn, the most popular guess is selected.</p>
            <p>▶ Past guesses are coloured to show which letters were in the correct position (green) or not in the corect position (yellow) or not in the word at all (grey).</p>
            <div class="center-align">
                <div class="example-guess">
                    <div v-for="guessletter in exampleGuess" :class="`guess-letter guess-letter-${guessletter.s}`">{{guessletter.c}}</div>
                </div>
            </div>
        </div>
    </div>
    <h3>Good luck!</h3>
</template>

<style scoped>
    h1,h2,h3 {
        text-align: center;
    }
    .instruction-card {
        border: 5px solid transparent;
    }
    .example-guess {
        text-align: center;
        display: inline-block;
    }
</style>

<script>
    export default {
        props: {
            maxLength: Number,
            maxGuesses: Number,
        },

        data() {
            return {
                timerId: null,
                guessTimeStart: Date.now(),
                guessDurationMS: 10000,
                guessTimePercent: 0,
                showBlankLetters: 0,
                exampleGuess: [{ s: 'correct', c: 'W' }, { s: 'absent', c: 'O' }, { s: 'correct', c: 'R' }, { s: 'present', c: 'D' }, { s: 'absent', c: 'S' }],
            }
        },

        methods: {
            timerUpdate() {
                var guessTimeFraction = ((Date.now() - this.guessTimeStart) % this.guessDurationMS) / this.guessDurationMS
                this.guessTimePercent = guessTimeFraction * 100

                // Count up and back within the guess duration.
                this.showBlankLetters = (guessTimeFraction > 0.5 ? 1 - guessTimeFraction : guessTimeFraction) * 2.5 * this.maxLength
            },
        },

        expose: [],

        mounted() {
            this.timerId = setInterval(this.timerUpdate, 50)
        },

        unmounted() {
            clearInterval(this.timerId)
        },
    }
</script>
