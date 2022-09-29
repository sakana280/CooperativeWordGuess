<template>
    <h1>Word Master Control Panel</h1>
    <fieldset :disabled="!isFormEnabled">
        <form v-on:submit.prevent="createGame">
            <label for="word">Word</label><input name="word" type="text" v-model.trim="word" maxlength="8" />
            <label for="guesses">Maximum number of guesses</label><input name="guesses" type="number" v-model.trim="maxGuesses" min="1" max="10" />
            <label for="duration">Duration per guess (seconds)</label><input name="duration" type="number" v-model.trim="guessDurationSeconds" min="5" max="500" />
            <button type="submit" class="btn waves-effect waves-light blue">Create Game</button>
        </form>
    </fieldset>
    <div><button :disabled="!isStartEnabled" type="submit" class="btn waves-effect waves-light blue" @click="startGame">Start Game</button></div>
    <p v-if="isError">Something went wrong. Could not create a new game.</p>
    <template v-if="publicToken">
        <!-- Use <a> instead of <reouter-link> to force opening in a new tab. -->
        <a :href="playUrl" target="_blank" rel="noopener noreferrer">Play this game</a><!--todo clipboard copy button-->
        <game :public-token="publicToken" />
    </template>
</template>

<style scoped>
    fieldset {
        display: contents; /* Hide border */
    }
    button {
        margin-top: 5px;
        margin-bottom: 7px;
    }
</style>

<script>
    import game from './game.vue'

    export default {
        data() {
            return {
                word: 'WORDS', //todo generate a random default
                maxGuesses: 6,
                guessDurationSeconds: 30,
                isFormEnabled: true,
                isStartEnabled: false,
                isError: false,
                adminToken: null,
                publicToken: null,
                playUrl: null,
            }
        },

        components: {
            game,
        },

        methods: {
            async createGame() {
                try {
                    this.isFormEnabled = false
                    var body = {
                        word: this.word,
                        maxGuesses: this.maxGuesses,
                        guessDurationSeconds: this.guessDurationSeconds,
                    }
                    const response = await fetch("/admin/CreateGame", {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(body),
                    })
                    if (!response.ok) {
                        throw new Error(response.statusText)
                    }
                    const created = await response.json()
                    this.isStartEnabled = true
                    this.adminToken = created.adminToken
                    this.publicToken = created.publicToken
                    this.playUrl = this.$router.resolve({ name: 'Play', params: { publicToken: created.publicToken } }).href
                    this.isStarted = true
                    //todo assign public key to game
                } catch (error) {
                    console.error(error)
                    this.isError = true
                    this.isFormEnabled = true
                }
            },

            async startGame() {
                try {
                    const response = await fetch(`/admin/StartGame?adminToken=${this.adminToken}&publicToken=${this.publicToken}`, {
                        method: 'POST',
                    })
                    if (!response.ok) {
                        throw new Error(response.statusText)
                    }
                    this.isStartEnabled = false
                } catch (error) {
                    console.error(error)
                    this.isError = true
                }
            },
        },

        expose: [],
    }

</script>
