module.exports = {
    isFunction(obj){
        return obj && {}.toString.call(obj) === '[object Function]';
    },
    isArray(arr){
        return arr.constructor === Array;
    },
}