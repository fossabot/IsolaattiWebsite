﻿Vue.component('users-grid', {
    props: {
        users: {
            type: Array,
            required: true
        }
    },
    methods: {
        imageUrl: function(imageId){
            if(imageId === null) {
                return "/res/imgs/user-solid.png";
            }
            return `/api/Fetch/ProfileImages/${imageId}.png`
        },
        navigateToProfile: function (profileId) {
            window.location = `/perfil/${profileId}`;
        }
    },
    template: `
      <div>
      <div class="users-grid">
        <div class="user-profile-card" v-for="user in users" @click="navigateToProfile(user.id)">
          <img :src="imageUrl(user.imageId)" class="profile-pic"/>
          <p class="text-ellipsis mw-100 m-0">{{ user.name }}</p>
        </div>

      </div>
      <p v-if="users.length < 1">
        No hay contenido que mostrar...
      </p>
      </div>
    `

})