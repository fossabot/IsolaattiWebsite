﻿const squadHeaderComponent = {
    props: {
        squadId: {
            required: true,
            type: String
        }
    },
    data: function() {
        return {
            customHeaders: customHttpHeaders,
            userData: userData,
            squadInfo: undefined,
            squadHeaderEditMode: false, // this should only be toggled by squad admin
            squadExtendedDescriptionCutContent: true,
            errorUpdating: false,
            submitting: false,
            squadInfoForEdit: {
                name: "",
                description: "",
                extendedDescription: ""
            },
            squadImageEdition: false
        }
    },
    computed:  {
        userIsAdmin: function() {
            if(this.squadInfo === undefined) return false;
            return this.squadInfo.userId === this.userData.id;
        },
        updateValidated: function() {
            return this.squadInfoForEdit.name.length > 0
                && this.squadInfoForEdit.extendedDescription.length > 0
                && this.squadInfoForEdit.description.length > 0;
        },
        extendedDescriptionCssClasses: function() {
            if(this.squadExtendedDescriptionCutContent) {
                return "post-cut-height"
            }
            return "overflow-auto"
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            return DOMPurify.sanitize(marked.parse(raw));
        },
        goToEditMode: function() {
            this.squadHeaderEditMode = !this.squadHeaderEditMode;
            this.squadInfoForEdit.name = this.squadInfo.name;
            this.squadInfoForEdit.description = this.squadInfo.description;
            this.squadInfoForEdit.extendedDescription = this.squadInfo.extendedDescription;
        },
        fetchSquad: async function() {
            const response = await fetch(`/api/Squads/${this.squadId}`, {
                headers: this.customHeaders
            });

            this.squadInfo = await response.json();
        },
        updateSquad: async function() {
            this.submitting = true;
            const response = await fetch(`/api/Squads/${this.squadId}/Update`, {
                method: "post",
                headers: this.customHeaders,
                body: JSON.stringify(this.squadInfoForEdit)
            });
            if(!response.ok) {
                this.errorUpdating = true;
                return;
            }
            const parsedResponse = await response.json();
            this.submitting = false;
            if(parsedResponse.result === "Success") {
                await this.fetchSquad();
                this.squadHeaderEditMode = false;
            }
        },
        toggleSquadImageEdition: function(){
            this.squadImageEdition = !this.squadImageEdition;
        }
    },
    mounted: async function() {
        await this.fetchSquad();
        this.squadExtendedDescriptionCutContent = 
            this.$refs.extendedDescriptionContainer.scrollHeight > this.$refs.extendedDescriptionContainer.clientHeight;
    },
    template: `
    <section v-if="squadInfo!==undefined">
    <div class="row m-0">
      <div class="col-12 d-flex justify-content-end">
        
      </div>
    </div>

    <div class="row m-0" v-if="!squadHeaderEditMode">
      <div class="col-lg-4">
        <template v-if="!squadImageEdition">
          <img src="" width="100" height="100" id="profile_photo" class="profile-pic">
          <h1>{{squadInfo.name}}</h1>
          <p>{{squadInfo.description}}</p>
          <div class="d-flex align-items-center w-100">
            <button class="btn btn-outline-primary w-100" @click="$router.push({path: '/miembros', query:{tab:'invitations', action:'new'}})">
              <i class="fa-solid fa-plus"></i> Invitar
            </button>
            <div class="dropdown ml-auto">
              <button class="btn" data-toggle="dropdown" aria-expanded="false" id="squad-dropdown">
                <i class="fa-solid fa-ellipsis"></i>
              </button>
              <div class="dropdown-menu" aria-labelledby="squad-dropdown">
                <button class="dropdown-item" v-if="userIsAdmin" @click="goToEditMode" href="#">Editar información</button>
                <button class="dropdown-item" @click="toggleSquadImageEdition">Cambiar imagen del squad</button>
                <a class="dropdown-item" href="#">Configuración</a>
                <div class="dropdown-divider"></div>
                <a class="dropdown-item" href="#">Salir</a>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <div class="d-flex">
            <button class="btn btn-light btn-sm" @click="toggleSquadImageEdition"><i class="fa-solid fa-arrow-left"></i></button>
          </div>
          <h3>Cambiar imagen del squad</h3>
          
          <profile-image-maker/>
        </template>
      </div>
      <div class="col-lg-8" ref="extendedDescriptionContainer" :class="extendedDescriptionCssClasses" v-html="compileMarkdown(squadInfo.extendedDescription)"></div>
      <div class="d-flex justify-content-center w-100">
        <button class="btn btn-link btn-sm" @click="squadExtendedDescriptionCutContent = !squadExtendedDescriptionCutContent">
          <span v-if="squadExtendedDescriptionCutContent">Mostrar todo</span>
          <span v-else>Contraer</span>
        </button>
      </div>
    </div>
    <div class="row m-0" v-else>
      <div class="d-flex flex-column w-100">
        <h3>Editar información del squad</h3>
        <div class="alert alert-danger" v-if="errorUpdating">Ocurrió un error al actualizar</div>
        <div class="form-group">
          <label for="name">Nombre</label>
          <input id="name" class="form-control" v-model="squadInfoForEdit.name"/>
        </div>
        <div class="form-group">
          <label for="description">Descripción</label>
          <input id="description" class="form-control" v-model="squadInfoForEdit.description"/>
        </div>
        <div class="form-group">
          <label for="extDescription">Descripción extendida</label>
          <textarea id="extDescription" class="form-control" v-model="squadInfoForEdit.extendedDescription" placeholder="Markdown es compatible" rows="10"></textarea>
        </div>
        <div class="d-flex justify-content-end mt-1">
          <button class="btn btn-light mr-1" @click="goToEditMode" :disabled="submitting">Cancelar</button>
          <button class="btn btn-primary" @click="updateSquad" :disabled="!updateValidated || submitting">Guardar</button>
        </div>
      </div>
    </div>
    </section>
    `
}

Vue.component('squad-header', squadHeaderComponent);