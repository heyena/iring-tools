// fLAB.js (file:// protocol adapter for LABjs 1.0+) | v0.2 (c) Kyle Simpson | MIT License
(function(u){var i=u.$LAB,q=u.document,w=q.location,r=(w.protocol==="file:");if(!i||!r){return}var A="undefined",p="string",o="head",x="body",t="function",n="script",c="srcuri",B="done",g="which",z=true,s=false,y=u.setTimeout,b=function(G){return q.getElementsByTagName(G)},E=Object.prototype.toString,l=function(){},f={},j={},h=/^[^?#]*\//.exec(w.href)[0],m=/^file:\/\/(localhost)?(\/[a-z]:)?/i.exec(h)[0],k=b(n),a=!+"\v1",e=a,F={dupe:s,preserve:s,base:"",which:o};f[o]=b(o);f[x]=b(x);function d(I,H){if(typeof I!==p){I=""}if(typeof H!==p){H=""}var G=(/^file\:\/\//.test(I)?"":H)+I;return((/^file\:\/\//.test(G)?"":(G.charAt(0)==="/"?m:h))+G)}function C(I){var H=0,G;while(G=k[H++]){if(typeof G.src===p&&I===d(G.src)){return z}}return s}function D(H){if(typeof H===A){H=F}var O=s,N=H.which,J=H.base,I=l,K=s,Q,L={},R=null;function M(T,W,U,X){if(f[T[g]][0]===null){y(arguments.callee,25);return}var S=q.createElement(n),V=function(Y,Z){S.setAttribute(Y,Z)};V("type",U);if(typeof X===p){V("charset",X)}V("src",W);f[T[g]][0].appendChild(S)}function G(X){if(typeof X.allowDup===A){X.allowDup=H.dupe}var W=X.src,V=X.type,Y=X.charset,T=X.allowDup,S=d(W,J),U;if(typeof V!==p){V="text/javascript"}if(typeof Y!==p){Y=null}T=!(!T);if(!T&&((typeof j[S]!==A&&j[S]!==null)||C(S))){return}if(typeof L[S]===A){L[S]={}}U=L[S];if(typeof U[g]===A){U[g]=N}U[B]=s;U[c]=S;K=z;j[U[c]]=z;M(U,S,V,Y)}function P(T){var S=[],U;for(U=0;U<T.length;U++){if(E.call(T[U])==="[object Array]"){S=S.concat(P(T[U]))}else{S[S.length]=T[U]}}return S}Q={script:function(){var T=P(arguments),S,U;for(U=0;U<T.length;U++){if(typeof T[U]===p){T[U]={src:d(T[U])}}else{if(typeof T[U].src!==A){T[U].src=d(T[U].src)}}}if(e){S=Q;for(U=0;U<T.length;U++){G(T[U])}}else{if(R===null){R=i.setOptions(H.pubMap)}S=R=R.script.apply(null,T)}return S},wait:function(T){var S;if(typeof T!==t){T=l}if(e){S=Q;y(T,0)}else{if(R===null){R=i.setOptions(H.pubMap)}S=R=R.wait(T)}return S}};Q.block=Q.wait;return Q}function v(K){var G,I={},H={AlwaysPreserveOrder:"preserve",AllowDuplicates:"dupe"},J={AppendTo:"which",BasePath:"base"};for(G in H){J[G]=H[G]}for(G in J){if(J.hasOwnProperty(G)&&typeof F[J[G]]!==A){I[J[G]]=(typeof K[G]!==A)?K[G]:F[J[G]]}}for(G in H){if(H.hasOwnProperty(G)){I[H[G]]=!(!I[H[G]])}}I.preload=I.cache=I.order=I.xhr=s;I.which=(I.which===o||I.which===x)?I.which:o;I.pubMap={};for(G in J){if(J.hasOwnProperty(G)){I.pubMap[G]=I[J[G]]}}return I}u.$LAB={setGlobalDefaults:function(G){F=v(G)},setOptions:function(G){return D(v(G))},script:function(){return D().script.apply(null,arguments)},wait:function(){return D().wait.apply(null,arguments)}};u.$LAB.block=u.$LAB.wait})(window);