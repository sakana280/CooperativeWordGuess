<template>
    <h1>Collaborative Word Guess</h1>
    <game :public-token="$route.params.publicToken" :guess-word="word" @ready="onReady" @playing="onPlaying" @guess="onGuess" @finished="onFinished" />
    <keyboard v-if="showKeyboard" v-model="word" :max-length="wordLength" :letter-states="letterStates" />
    <instructions v-if="showInstructions" :max-length="wordLength" :max-guesses="maxGuesses" />
</template>

<style scoped>
    h1, p {
        text-align: center;
    }
</style>

<script>
    import game from './game.vue'
    import keyboard from './keyboard.vue'
    import instructions from './instructions.vue'

    export default {
        data() {
            return {
                word: null,
                wordLength: null,
                maxGuesses: null,
                letterStates: [],
                showInstructions: false,
                showKeyboard: false,
            }
        },

        components: {
            game,
            keyboard,
            instructions,
        },

        methods: {
            onReady(wordLength, maxGuesses) {
                this.wordLength = wordLength
                this.maxGuesses = maxGuesses
                this.showInstructions = true
            },

            onPlaying() {
                this.showInstructions = false
                this.showKeyboard = true
            },

            onGuess(letterStates) {
                this.word = null
                this.letterStates = letterStates
            },

            onFinished() {
                this.showInstructions = false
                this.showKeyboard = false
            },
        },
    }

</script>
