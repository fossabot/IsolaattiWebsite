Vue.component('comment', {
    props: ['comment'],
    data: function () {
        return {
            userData: userData,
            cutContent: true,
            edit: false,
            deleteMode: false,
            customHeaders: customHttpHeaders,
        }
    },
    computed: {
        profileLink: function () {
            return `/perfil/${this.comment.authorId}`
        },
        reportLink: function () {
            return `/Reports/ReportPostOrComment?commentId=${this.comment.id}`;
        },
        containerCssClass: function () {
            return this.cutContent ? "post d-flex mt-2 flex-column p-2 post-cut-height" : "post d-flex mt-2 flex-column p-2"
        },
        titleAtr: function () {
            return `id del comentario: ${this.comment.id}`
        },
        rootContainerCss: function () {
            return this.edit || this.deleteMode ? "background-color: #f8f9fa; padding:0.2rem" : "";
        }
    },
    methods: {
        compileMarkdown: function (raw) {
            if (raw !== null)
                return DOMPurify.sanitize(marked.parse(raw));
        },
        getUserImageUrl: function (userId) {
            return `/api/Fetch/GetUserProfileImage?userId=${userId}`
        },
        showFullPost: function () {
            this.cutContent = false;
        },
        deleteResponse: function () {
            const that = this;
            fetch("/api/Comment/Delete", {
                method: "POST",
                headers: this.customHeaders,
                body: JSON.stringify({
                    id: this.comment.id
                })
            }).then(_ => {
                this.deleteMode = false;
                that.$emit("commentDeleted", that.comment.id);
            });
        }
    },
    template: `
      <div :style="rootContainerCss" class="mt-1">
      <section v-if="edit" class="d-flex justify-content-end p-1">
        <button @click="edit=false" class="btn btn-danger btn-sm">Cancelar edición</button>
      </section>
      <section v-if="deleteMode" class="w-100 mt-2 d-flex justify-content-end align-items-center p-1">
        <span class="mr-auto">¿Eliminar esta respuesta?</span>
        <button class="btn btn-light mr-1" @click="deleteMode=false">No</button>
        <button class="btn btn-primary" @click="deleteResponse">Sí</button>
      </section>
      <article v-if="!edit" class="d-flex align-items-center">
        <div class="comments-start-line"></div>
        <div :class="containerCssClass" :title="titleAtr">
          <div class="d-flex justify-content-between align-items-center">
            <div class="d-flex">
              <img class="user-avatar" :src="getUserImageUrl(comment.authorId)">
              <div class="d-flex flex-column ml-2">
                <span class="user-name"><a :href="profileLink">{{ comment.authorName }}</a> </span>
                <span>{{ new Date(comment.timeStamp).toUTCString() }}</span>
              </div>
            </div>
            <div class="dropdown dropleft" v-if="userData.id!==-1">
              <button class="btn btn-light btn-sm" data-toggle="dropdown" aria-haspopup="true">
                <i class="fas fa-ellipsis-h"></i>
              </button>
              <div class="dropdown-menu">
                <a href="#" class="dropdown-item" v-if="comment.authorId===this.userData.id"
                   @click="edit=true">Editar</a>
                <a href="#" class="dropdown-item" v-if="comment.authorId===this.userData.id"
                   v-on:click="deleteMode=true">Eliminar</a>
                <a :href="reportLink" class="dropdown-item" target="_blank">Reportar</a>
              </div>
            </div>
          </div>
          <audio-attachment v-if="comment.audioId!==null" :audio-id="comment.audioId"></audio-attachment>
          <div class="mt-2 post-content" v-html="compileMarkdown(comment.content)" ref="commentContentContainer"></div>
          <div class="d-flex justify-content-center">
            <button class="btn btn-primary " v-on:click="showFullPost" v-if="cutContent">Mostrar todo</button>
          </div>
        </div>
      </article>
      <section v-else>
        <new-comment :post-to-comment="comment.postId" :mode="'modify'" :comment-to-edit="comment.id"
                     @commentEdited="edit=false"></new-comment>
      </section>
      </div>
    `,
    mounted: function () {
        this.cutContent = this.$refs.commentContentContainer.scrollHeight > this.$refs.commentContentContainer.clientHeight;
    }
})